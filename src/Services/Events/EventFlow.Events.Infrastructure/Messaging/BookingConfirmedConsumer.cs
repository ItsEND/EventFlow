using Confluent.Kafka;
using EventFlow.Contracts;
using EventFlow.Events.Infrastructure.Messaging.Inbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace EventFlow.Events.Infrastructure.Messaging;

public sealed class BookingConfirmedConsumer(IServiceScopeFactory scopeFactory, IOptions<KafkaOptions> options, ILogger<BookingConfirmedConsumer> logger)
    : BackgroundService
{
    private const int MaxProcessingAttempts = 3;

    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(2);
    private readonly KafkaOptions _options = options.Value;
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ValidateOptions();

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = _options.ConsumerGroup,

            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnableAutoOffsetStore = false
        };

        var deadLetterProducerConfig = new ProducerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            Acks = Acks.All,
            EnableIdempotence = true
        };
        using var consumer = new ConsumerBuilder<string, string>(consumerConfig)
            .SetErrorHandler((_, error) =>
                logger.Log(
                    error.IsFatal ? LogLevel.Critical : LogLevel.Warning,
                    "Ошибка Kafka consumer {Code}: {Reason}",
                    error.Code,
                    error.Reason))
            .SetLogHandler((_, message) =>
                logger.Log(
                    MapKafkaLogLevel(message.Level),
                    "Kafka consumer {KafkaLevel} {Facility}: {Message}",
                    message.Level,
                    message.Facility,
                    message.Message))
            .Build();

        using var deadLetterProducer = new ProducerBuilder<string, string>(deadLetterProducerConfig)
            .SetErrorHandler((_, error) =>
                logger.Log(
                    error.IsFatal ? LogLevel.Critical : LogLevel.Warning,
                    "Ошибка Kafka DLQ producer {Code}: {Reason}",
                    error.Code,
                    error.Reason))
            .SetLogHandler((_, message) =>
                logger.Log(
                    MapKafkaLogLevel(message.Level),
                    "Kafka DLQ producer {KafkaLevel} {Facility}: {Message}",
                    message.Level,
                    message.Facility,
                    message.Message))
            .Build();

        consumer.Subscribe(KafkaTopics.BookingConfirmed);

        logger.LogInformation("Потребитель Kafka подписан на топик {Topic}", KafkaTopics.BookingConfirmed);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<string, string> consumeResult;

                try
                {
                    consumeResult = consumer.Consume(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (ConsumeException exception)
                {
                    logger.LogError(exception, "Ошибка при чтении сообщения из Kafka");

                    continue;
                }
                try
                {
                    var message = DeserializeMessage(consumeResult);

                    if (!IsValid(message))
                    {
                        throw new InvalidDataException("Сообщение BookingConfirmed содержит некорректные данные.");
                    }

                    await ProcessWithRetryAsync(message, stoppingToken);

                    await CommitWithRetryAsync(consumer, consumeResult, stoppingToken);

                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    await MoveToDeadLetterAsync(deadLetterProducer, consumer, consumeResult, exception, stoppingToken);
                }
            }
        }
        finally
        {
            consumer.Close();
        }

    }
    private async Task ProcessWithRetryAsync(BookingConfirmed message, CancellationToken cancellationToken)
    {
        Exception? lastException = null;

        for (var attempt = 1; attempt <= MaxProcessingAttempts; attempt++)
        {
            try
            {
                await ProcessMessageAsync(message, cancellationToken);
                return;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                lastException = exception;

                logger.LogWarning(
                    exception,
                    "Попытка {Attempt}/{MaxAttempts} обработки брони {BookingId} завершилась ошибкой",
                    attempt,
                    MaxProcessingAttempts,
                    message.BookingId);

                if (attempt < MaxProcessingAttempts)
                {
                    await Task.Delay(RetryDelay, cancellationToken);
                }
            }
        }
        throw new InvalidOperationException(
             $"Не удалось обработать бронь {message.BookingId} " +
             $"после {MaxProcessingAttempts} попыток.",
             lastException);

    }
    private async Task ProcessMessageAsync(BookingConfirmed message, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var handler = scope.ServiceProvider.GetRequiredService<BookingConfirmedInboxHandler>();

        await handler.HandleAsync(message, cancellationToken);
    }

    private async Task MoveToDeadLetterAsync(IProducer<string, string> deadLetterProducer, IConsumer<string, string> consumer, ConsumeResult<string, string> consumeResult, Exception processingException, CancellationToken cancellationToken)
    {
        var deadLetterMessage = new Message<string, string>
        {
            Key = consumeResult.Message.Key,
            Value = consumeResult.Message.Value,
            Headers = new Headers
            {
                {
                    "error-type",
                    Encode(processingException.GetType().Name)
                },
                {
                    "error-reason",
                    Encode(processingException.Message)
                },
                {
                    "source-topic",
                    Encode(consumeResult.Topic)
                },
                {
                    "source-partition",
                    Encode(consumeResult.Partition.Value.ToString())
                },
                {
                    "source-offset",
                    Encode(consumeResult.Offset.Value.ToString())
                },
                {
                    "failed-at",
                    Encode(DateTime.UtcNow.ToString("O"))
                }
            }
        };

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await deadLetterProducer.ProduceAsync(KafkaTopics.BookingConfirmedDeadLetter, deadLetterMessage, cancellationToken);
                logger.LogError(
                   processingException,
                   "Сообщение из топика {Topic}, раздела {Partition}, " +
                   "со смещением {Offset} перемещено в топик ошибок {DeadLetterTopic}",
                   consumeResult.Topic,
                   consumeResult.Partition.Value,
                   consumeResult.Offset.Value,
                   KafkaTopics.BookingConfirmedDeadLetter);

                break;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }

            catch (Exception deadLetterException)
            {
                logger.LogError(
                    deadLetterException,
                    "Не удалось отправить сообщение в топик ошибок {DeadLetterTopic}. " +
                    "Операция будет повторена",
                    KafkaTopics.BookingConfirmedDeadLetter);

                await Task.Delay(RetryDelay, cancellationToken);
            }
        }
        await CommitWithRetryAsync(consumer, consumeResult, cancellationToken);
    }
    private async Task CommitWithRetryAsync(IConsumer<string, string> consumer, ConsumeResult<string, string> consumeResult, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                consumer.Commit(consumeResult);
                return;
            }
            catch (KafkaException exception)
            {
                logger.LogError(
                    exception,
                    "Не удалось подтвердить смещение Kafka {Offset}. " +
                    "Операция будет повторена",
                    consumeResult.Offset.Value);

                await Task.Delay(RetryDelay, cancellationToken);
            }
            cancellationToken.ThrowIfCancellationRequested();
        }
    }

    private BookingConfirmed DeserializeMessage(ConsumeResult<string, string> consumeResult)
    {
        return string.IsNullOrWhiteSpace(consumeResult.Message.Value)
            ? throw new JsonException("Тело сообщения Kafka пусто.")
            : JsonSerializer.Deserialize<BookingConfirmed>(consumeResult.Message.Value)
            ?? throw new JsonException("Сообщение BookingConfirmed пусто.");
    }



    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(_options.BootstrapServers))
        {
            throw new InvalidOperationException("Адрес брокера Kafka (BootstrapServers) не настроен.");
        }

        if (string.IsNullOrWhiteSpace(_options.ConsumerGroup))
        {
            throw new InvalidOperationException("Группа потребителей Kafka (ConsumerGroup) не настроена.");
        }
    }


    private static bool IsValid(BookingConfirmed message)
    {
        return message.BookingId != Guid.Empty && message.EventId != Guid.Empty
            && message.UserId != Guid.Empty && message.SeatCount > 0;
    }

    private static byte[] Encode(string value)
    {
        return Encoding.UTF8.GetBytes(value);
    }

    private static LogLevel MapKafkaLogLevel(SyslogLevel level) => level switch
    {
        SyslogLevel.Emergency or
        SyslogLevel.Alert or
        SyslogLevel.Critical => LogLevel.Critical,
        SyslogLevel.Error => LogLevel.Error,
        SyslogLevel.Warning => LogLevel.Warning,
        SyslogLevel.Notice or SyslogLevel.Info => LogLevel.Information,
        SyslogLevel.Debug => LogLevel.Debug,
        _ => LogLevel.Information
    };
}

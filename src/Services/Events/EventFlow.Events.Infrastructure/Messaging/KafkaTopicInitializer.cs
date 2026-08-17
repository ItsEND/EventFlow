using Confluent.Kafka;
using Confluent.Kafka.Admin;
using EventFlow.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventFlow.Events.Infrastructure.Messaging;

public sealed class KafkaTopicInitializer(IOptions<KafkaOptions> options, ILogger<KafkaTopicInitializer> logger) : IHostedService
{
    private readonly KafkaOptions _options = options.Value;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.BootstrapServers))
        {
            logger.LogWarning("Адрес брокера Kafka (BootstrapServers) не настроен");

            return;
        }
        var adminConfig = new AdminClientConfig
        {
            BootstrapServers = _options.BootstrapServers,
        };

        using var adminClient = new AdminClientBuilder(adminConfig)
            .SetErrorHandler((_, error) =>
                logger.Log(
                    error.IsFatal ? LogLevel.Critical : LogLevel.Warning,
                    "Ошибка Kafka AdminClient {Code}: {Reason}",
                    error.Code,
                    error.Reason))
            .SetLogHandler((_, message) =>
                logger.Log(
                    MapKafkaLogLevel(message.Level),
                    "Kafka AdminClient {KafkaLevel} {Facility}: {Message}",
                    message.Level,
                    message.Facility,
                    message.Message))
            .Build();

        await EnsureTopicExistsAsync(adminClient, KafkaTopics.BookingConfirmed);

        await EnsureTopicExistsAsync(adminClient, KafkaTopics.BookingConfirmedDeadLetter);
    }

    private async Task EnsureTopicExistsAsync(IAdminClient adminClient, string topicName)
    {
        try
        {
            await adminClient.CreateTopicsAsync([
                new TopicSpecification{
                    Name = topicName,
                    NumPartitions = 1,
                    ReplicationFactor = 1
                }
                ]);
            logger.LogInformation("Топик Kafka {Topic} создан",
             topicName);
        }
        catch (CreateTopicsException exception) when (exception.Results.All(result => result.Error.Code == ErrorCode.TopicAlreadyExists))
        {
            logger.LogInformation("Топик Kafka {Topic} уже существует",
              topicName);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Не удалось создать топик Kafka {Topic}",
                topicName);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
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


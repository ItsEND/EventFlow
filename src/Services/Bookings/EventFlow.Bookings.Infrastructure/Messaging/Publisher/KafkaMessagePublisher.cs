using Confluent.Kafka;
using EventFlow.Bookings.Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventFlow.Bookings.Infrastructure.Messaging.Publisher;

public sealed class KafkaMessagePublisher : IMessagePublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;

    public KafkaMessagePublisher(IOptions<KafkaOptions> options, ILogger<KafkaMessagePublisher> logger)
    {
        var bootstrapServers = options.Value.BootstrapServers;

        if (string.IsNullOrWhiteSpace(bootstrapServers))
        {
            throw new InvalidOperationException("Адрес брокера Kafka (BootstrapServers) не настроен.");
        }

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            Acks = Acks.All,
            EnableIdempotence = true
        };

        _producer = new ProducerBuilder<string, string>(producerConfig)
            .SetErrorHandler((_, error) =>
                logger.Log(
                    error.IsFatal ? LogLevel.Critical : LogLevel.Warning,
                    "Ошибка Kafka producer {Code}: {Reason}",
                    error.Code,
                    error.Reason))
            .SetLogHandler((_, message) =>
                logger.Log(
                    MapKafkaLogLevel(message.Level),
                    "Kafka producer {KafkaLevel} {Facility}: {Message}",
                    message.Level,
                    message.Facility,
                    message.Message))
            .Build();
    }

    public async Task PublishAsync(string topic, string messageKey, string payload, CancellationToken cancellationToken)
    {
        var kafkaMessage = new Message<string, string>
        {
            Key = messageKey,
            Value = payload
        };

        await _producer.ProduceAsync(topic, kafkaMessage, cancellationToken);
    }
    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
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

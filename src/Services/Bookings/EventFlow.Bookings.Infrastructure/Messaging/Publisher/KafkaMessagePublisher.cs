using Confluent.Kafka;
using EventFlow.Bookings.Application.Abstractions.Messaging;
using Microsoft.Extensions.Options;

namespace EventFlow.Bookings.Infrastructure.Messaging.Publisher;

public sealed class KafkaMessagePublisher : IMessagePublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;

    public KafkaMessagePublisher(IOptions<KafkaOptions> options)
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

        _producer = new ProducerBuilder<string, string>(producerConfig).Build();
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
}
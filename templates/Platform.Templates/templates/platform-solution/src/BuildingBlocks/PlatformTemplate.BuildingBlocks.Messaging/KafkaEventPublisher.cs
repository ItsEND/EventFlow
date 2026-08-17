using Confluent.Kafka;
using System.Text.Json;

namespace PlatformTemplate.BuildingBlocks.Messaging;

internal sealed class KafkaEventPublisher(IProducer<string, string> producer) : IEventPublisher
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public Task PublishAsync<T>(string topic, string key, T message, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(message, SerializerOptions);
        return PublishSerializedAsync(topic, key, payload, cancellationToken);
    }

    public async Task PublishSerializedAsync(string topic, string key, string payload, CancellationToken cancellationToken = default)
    {
        await producer.ProduceAsync(
            topic,
            new Message<string, string>
            {
                Key = key,
                Value = payload
            },
            cancellationToken);
    }
}

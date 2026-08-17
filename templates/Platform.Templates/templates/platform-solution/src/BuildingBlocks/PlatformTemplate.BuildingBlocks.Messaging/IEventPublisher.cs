namespace PlatformTemplate.BuildingBlocks.Messaging;

public interface IEventPublisher
{
    Task PublishAsync<T>(string topic, string key, T message, CancellationToken cancellationToken = default);

    Task PublishSerializedAsync(string topic, string key, string payload, CancellationToken cancellationToken = default);
}

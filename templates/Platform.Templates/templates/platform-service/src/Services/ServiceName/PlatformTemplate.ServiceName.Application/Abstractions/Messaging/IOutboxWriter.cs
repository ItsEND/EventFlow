namespace PlatformTemplate.ServiceName.Application.Abstractions.Messaging;

public interface IOutboxWriter
{
    void Write<T>(string topic, string key, T message);
}

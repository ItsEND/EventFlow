using PlatformTemplate.ServiceName.Application.Abstractions.Messaging;
using PlatformTemplate.ServiceName.Infrastructure.DataAccess;
using System.Text.Json;

namespace PlatformTemplate.ServiceName.Infrastructure.Messaging.Outbox;

internal sealed class OutboxWriter(ServiceNameDbContext dbContext) : IOutboxWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public void Write<T>(string topic, string key, T message)
    {
        var payload = JsonSerializer.Serialize(message, SerializerOptions);
        dbContext.OutboxMessages.Add(OutboxMessage.Create(topic, key, payload));
    }
}

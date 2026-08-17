namespace PlatformTemplate.ServiceName.Infrastructure.Messaging.Outbox;

internal sealed class OutboxMessage
{
    private OutboxMessage()
    {
    }

    private OutboxMessage(Guid id, string topic, string key, string payload, DateTimeOffset occurredAt)
    {
        Id = id;
        Topic = topic;
        Key = key;
        Payload = payload;
        OccurredAt = occurredAt;
    }

    public Guid Id { get; private set; }

    public string Topic { get; private set; } = string.Empty;

    public string Key { get; private set; } = string.Empty;

    public string Payload { get; private set; } = string.Empty;

    public DateTimeOffset OccurredAt { get; private set; }

    public DateTimeOffset? PublishedAt { get; private set; }

    public int Attempts { get; private set; }

    public string? LastError { get; private set; }

    public static OutboxMessage Create(string topic, string key, string payload) =>
        new(Guid.NewGuid(), topic, key, payload, DateTimeOffset.UtcNow);

    public void MarkPublished(DateTimeOffset publishedAt)
    {
        PublishedAt = publishedAt;
        LastError = null;
    }

    public void MarkFailed(string error)
    {
        Attempts++;
        LastError = error.Length <= 2000 ? error : error[..2000];
    }
}

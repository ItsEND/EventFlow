namespace EventFlow.Bookings.Infrastructure.Messaging.Outbox;

public sealed class OutboxMessage
{
    private const int MaxErrorLength = 2000;
    public Guid Id { get; private set; }
    public string Topic { get; private set; } = null!;
    public string MessageKey { get; private set; } = null!;
    public string Payload { get; private set; } = null!;
    public DateTime OccurredAt { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public int Attempts { get; private set; }
    public string? LastError { get; private set; }

    private OutboxMessage()
    {
        // Конструктор для EF Core.
    }

    public static OutboxMessage Create(Guid id, string topic, string messageKey, string payload, DateTime occurredAt)
    {
        return id == Guid.Empty
            ? throw new ArgumentException("Идентификатор Outbox-сообщения не может быть пустым.", nameof(id))
            : string.IsNullOrWhiteSpace(topic)
            ? throw new ArgumentException("Имя Kafka-топика не может быть пустым.", nameof(topic))
            : string.IsNullOrWhiteSpace(messageKey)
            ? throw new ArgumentException("Ключ Kafka-сообщения не может быть пустым.", nameof(messageKey))
            : string.IsNullOrWhiteSpace(payload)
            ? throw new ArgumentException("Тело Outbox-сообщения не может быть пустым.", nameof(payload))
            : new OutboxMessage
            {
                Id = id,
                Topic = topic,
                MessageKey = messageKey,
                Payload = payload,
                OccurredAt = occurredAt
            };
    }

    public void MarkPublished(DateTime publishedAt)
    {
        PublishedAt = publishedAt;
        LastError = null;
    }

    public void RegisterFailure(string error)
    {
        Attempts++;

        LastError = error.Length <= MaxErrorLength
            ? error
            : error[..MaxErrorLength];
    }
}
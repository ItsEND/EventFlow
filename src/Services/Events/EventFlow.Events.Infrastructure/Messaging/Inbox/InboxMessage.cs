namespace EventFlow.Events.Infrastructure.Messaging.Inbox;

public sealed class InboxMessage
{
    private const int MaxDetailsLength = 1000;
    public Guid MessageId { get; private set; }
    public DateTime ReceivedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public InboxMessageStatus Status { get; private set; }
    public string? Details { get; private set; }

    private InboxMessage()
    {
        // Конструктор для EF Core.
    }

    public static InboxMessage Create(Guid messageId, DateTime receivedAt)
    {
        return messageId == Guid.Empty
            ? throw new ArgumentException("Идентификатор Inbox-сообщения не может быть пустым.", nameof(messageId))
            : new InboxMessage
            {
                MessageId = messageId,
                ReceivedAt = receivedAt,
                Status = InboxMessageStatus.Pending
            };
    }

    public void MarkProcessed(DateTime processedAt)
    {
        Status = InboxMessageStatus.Processed;
        ProcessedAt = processedAt;
        Details = null;
    }

    public void MarkIgnored(InboxMessageStatus status, string details, DateTime processedAt)
    {
        if (status is not InboxMessageStatus.IgnoredEventNotFound and not InboxMessageStatus.IgnoredNotEnoughSeats)
        {
            throw new ArgumentException("Для пропущенного сообщения указан некорректный статус.", nameof(status));
        }

        Status = status;
        ProcessedAt = processedAt;

        Details = details.Length <= MaxDetailsLength
            ? details
            : details[..MaxDetailsLength];
    }
}
namespace PlatformTemplate.ServiceName.Infrastructure.Messaging.Inbox;

internal sealed class InboxMessage
{
    private InboxMessage()
    {
    }

    private InboxMessage(Guid messageId, string eventType, string payload, DateTimeOffset receivedAt)
    {
        MessageId = messageId;
        EventType = eventType;
        Payload = payload;
        ReceivedAt = receivedAt;
        Status = InboxMessageStatus.Processing;
    }

    public Guid MessageId { get; private set; }

    public string EventType { get; private set; } = string.Empty;

    public string Payload { get; private set; } = string.Empty;

    public DateTimeOffset ReceivedAt { get; private set; }

    public DateTimeOffset? ProcessedAt { get; private set; }

    public InboxMessageStatus Status { get; private set; }

    public string? Error { get; private set; }

    public static InboxMessage Start(Guid messageId, string eventType, string payload) =>
        new(messageId, eventType, payload, DateTimeOffset.UtcNow);

    public void MarkProcessed()
    {
        Status = InboxMessageStatus.Processed;
        ProcessedAt = DateTimeOffset.UtcNow;
        Error = null;
    }

    public void MarkFailed(string error)
    {
        Status = InboxMessageStatus.Failed;
        Error = error.Length <= 2000 ? error : error[..2000];
    }
}

namespace EventFlow.Events.Infrastructure.Messaging.Inbox;

public enum InboxMessageStatus
{
    Pending,
    Processed,
    IgnoredEventNotFound,
    IgnoredNotEnoughSeats
}

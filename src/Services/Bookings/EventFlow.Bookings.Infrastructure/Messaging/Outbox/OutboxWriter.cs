using EventFlow.Bookings.Application.Abstractions.Messaging.Outbox;
using EventFlow.Bookings.Infrastructure.DataAccess;
using EventFlow.Contracts;
using System.Text.Json;

namespace EventFlow.Bookings.Infrastructure.Messaging.Outbox;

public sealed class OutboxWriter(BookingDbContext dbContext) : IOutboxWriter
{
    public void Add(BookingConfirmed message)
    {
        var payload = JsonSerializer.Serialize(message);

        var outboxMessage = OutboxMessage.Create(
            id: message.BookingId,
            topic: KafkaTopics.BookingConfirmed,
            messageKey: message.EventId.ToString(),
            payload: payload,
            occurredAt: message.ConfirmedAt);

        dbContext.OutboxMessages.Add(outboxMessage);
    }
}

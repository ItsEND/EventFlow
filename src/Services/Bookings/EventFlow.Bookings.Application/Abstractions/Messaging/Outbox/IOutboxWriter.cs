using EventFlow.Contracts;

namespace EventFlow.Bookings.Application.Abstractions.Messaging.Outbox;

public interface IOutboxWriter
{
    void Add(BookingConfirmed message);
}

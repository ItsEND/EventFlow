namespace EventFlow.Api.Models;

public class Booking
{
    public Guid Id { get; init; }
    public Guid EventId { get; init; }
    public BookingStatus Status { get; private set; } = BookingStatus.Pending;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; private set; }

    public Booking(Guid id, Guid eventId)
    {
        Id = id;
        EventId = eventId;
    }

    public static Booking Create(Guid eventId)
    {
        return new Booking(Guid.NewGuid(), eventId);
    }

    public void Update(BookingStatus status)
    {
        Status = status;
        ProcessedAt = DateTime.UtcNow;
    }

}

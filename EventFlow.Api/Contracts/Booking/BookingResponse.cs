using EventFlow.Api.Models;

namespace EventFlow.Api.Contracts.Booking;

public class BookingResponse
{
    public Guid Id { get; init; }
    public Guid EventId { get; init; }
    public BookingStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ProcessedAt { get; set; }

}
namespace EventFlow.Api.Contracts.Booking;

public record class BookingResponse
{
    public Guid Id { get; init; }
    public Guid EventId { get; init; }
    public string Status { get; init; }

}
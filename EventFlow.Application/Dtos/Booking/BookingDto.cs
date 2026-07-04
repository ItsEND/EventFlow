namespace EventFlow.Application.Dtos.Booking;

/// <summary>
/// DTO брони, которое Application отдает внешним слоям.
/// </summary>
public record class BookingDto
{
    public required Guid Id { get; init; }
    public required Guid EventId { get; init; }
    public required string Status { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? ProcessedAt { get; init; }
}

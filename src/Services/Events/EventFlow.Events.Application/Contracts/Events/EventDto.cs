namespace EventFlow.Events.Application.Contracts.Events;

/// <summary>
/// DTO мероприятия, которое Application отдает внешним слоям.
/// </summary>
public record class EventDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public required int TotalSeats { get; init; }
    public required int AvailableSeats { get; init; }
    public required DateTime StartAt { get; init; }
    public required DateTime EndAt { get; init; }
}

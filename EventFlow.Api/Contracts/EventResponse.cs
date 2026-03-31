namespace EventFlow.Api.Contracts;

public record class EventResponse
{
    public required Guid Id { get; init; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required DateTime StartAt { get; set; }
    public required DateTime EndAt { get; set; }
}

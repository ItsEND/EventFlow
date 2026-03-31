namespace EventFlow.Api.Models;

public record class CreateEventModel
{
    public required string Title { get; init; }
    public string? Description { get; init; }
    public required DateTime StartAt { get; init; }
    public required DateTime EndAt { get; init; }
}

namespace EventFlow.Api.Models;

public record class UpdateEventModel
{
    public string Title { get; init; }
    public string? Description { get; init; }
    public DateTime StartAt { get; init; }
    public DateTime EndAt { get; init; }
}

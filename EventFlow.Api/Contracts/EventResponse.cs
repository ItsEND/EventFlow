namespace EventFlow.Api.Contracts;

/// <summary>
/// Ответ с данными мероприятия.
/// </summary>
public record class EventResponse
{
    /// <summary>
    /// Идентификатор мероприятия.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Название мероприятия.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Описание мероприятия.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Дата и время начала мероприятия.
    /// </summary>
    public required DateTime StartAt { get; init; }

    /// <summary>
    /// Дата и время окончания мероприятия.
    /// </summary>
    public required DateTime EndAt { get; init; }
}

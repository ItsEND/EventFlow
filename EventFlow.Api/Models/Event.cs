namespace EventFlow.Api.Models;

/// <summary>
/// Мероприятие.
/// </summary>
public class Event
{
    /// <summary>
    /// Идентификатор мероприятия.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Название мероприятия.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Описание мероприятия.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Дата и время начала мероприятия.
    /// </summary>
    public required DateTime StartAt { get; set; }

    /// <summary>
    /// Дата и время окончания мероприятия.
    /// </summary>
    public required DateTime EndAt { get; set; }
}

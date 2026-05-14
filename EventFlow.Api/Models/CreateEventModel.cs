namespace EventFlow.Api.Models;

/// <summary>
/// Внутренняя модель для создания мероприятия.
/// </summary>
public record class CreateEventModel
{
    /// <summary>
    /// Название мероприятия.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Описание мероприятия.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Общее количество мест на событии
    /// </summary>
    public int TotalSeats { get; init; }

    /// <summary>
    /// Дата и время начала мероприятия.
    /// </summary>
    public required DateTime StartAt { get; init; }

    /// <summary>
    /// Дата и время окончания мероприятия.
    /// </summary>
    public required DateTime EndAt { get; init; }
}

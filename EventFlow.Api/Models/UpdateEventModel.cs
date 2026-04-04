namespace EventFlow.Api.Models;

/// <summary>
/// Внутренняя модель для обновления мероприятия.
/// </summary>
public record class UpdateEventModel
{
    /// <summary>
    /// Название мероприятия.
    /// </summary>
    public string Title { get; init; }

    /// <summary>
    /// Описание мероприятия.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Дата и время начала мероприятия.
    /// </summary>
    public DateTime StartAt { get; init; }

    /// <summary>
    /// Дата и время окончания мероприятия.
    /// </summary>
    public DateTime EndAt { get; init; }
}

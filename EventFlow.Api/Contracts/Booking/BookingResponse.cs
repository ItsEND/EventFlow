namespace EventFlow.Api.Contracts.Booking;

/// <summary>
/// Ответ с текущим состоянием брони.
/// </summary>
public record class BookingResponse
{
    /// <summary>
    /// Идентификатор брони.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Идентификатор мероприятия, к которому относится бронь.
    /// </summary>
    public required Guid EventId { get; init; }

    /// <summary>
    /// Текущий статус брони.
    /// </summary>
    public required string Status { get; init; }

    /// <summary>
    /// Дата и время создания брони.
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Дата и время обработки брони.
    /// Значение отсутствует, пока бронь находится в статусе Pending.
    /// </summary>
    public DateTime? ProcessedAt { get; init; }
}
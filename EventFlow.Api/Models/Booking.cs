using System.ComponentModel.DataAnnotations;

namespace EventFlow.Api.Models;

/// <summary>
/// Бронь на мероприятие.
/// Создаётся в статусе Pending и после фоновой обработки переходит
/// в один из терминальных статусов: Confirmed или Rejected.
/// </summary>
public class Booking
{
    /// <summary>
    /// Уникальный идентификатор брони.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Идентификатор мероприятия, к которому относится бронь.
    /// </summary>
    public Event Event { get; private set; } = null!;
    public Guid EventId { get; init; }

    /// <summary>
    /// Текущий статус брони.
    /// </summary>
    public BookingStatus Status { get; private set; } = BookingStatus.Pending;

    /// <summary>
    /// Дата и время создания брони в UTC.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Дата и время обработки брони в UTC.
    /// Заполняется при переходе в Confirmed или Rejected.
    /// </summary>
    public DateTime? ProcessedAt { get; private set; }

    private Booking() { } // Для EF Core
    private Booking(Guid id, Guid eventId)
    {
        Id = id;
        EventId = eventId;
    }

    /// <summary>
    /// Создаёт новую бронь в статусе Pending.
    /// </summary>
    /// <param name="eventId">Идентификатор мероприятия.</param>
    /// <returns>Созданная бронь.</returns>
    /// <exception cref="ValidationException">
    /// Выбрасывается, если передан пустой идентификатор мероприятия.
    /// </exception>
    public static Booking Create(Guid eventId)
    {
        return eventId == Guid.Empty
            ? throw new ValidationException("Идентификатор мероприятия не может быть пустым.")
            : new Booking(Guid.NewGuid(), eventId);
    }

    /// <summary>
    /// Подтверждает бронь.
    /// </summary>
    /// <exception cref="ValidationException">
    /// Выбрасывается, если бронь уже была обработана.
    /// </exception>
    public void Confirm()
    {
        Complete(BookingStatus.Confirmed);
    }

    /// <summary>
    /// Отклоняет бронь.
    /// </summary>
    /// <exception cref="ValidationException">
    /// Выбрасывается, если бронь уже была обработана.
    /// </exception>
    public void Reject()
    {
        Complete(BookingStatus.Rejected);
    }

    private void Complete(BookingStatus targetStatus)
    {
        if (Status != BookingStatus.Pending)
        {
            throw new ValidationException(
                $"Бронь уже обработана. Текущий статус: {Status}.");
        }

        Status = targetStatus;
        ProcessedAt = DateTime.UtcNow;
    }
}
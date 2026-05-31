using System.ComponentModel.DataAnnotations;

namespace EventFlow.Api.Models;

/// <summary>
/// Мероприятие.
/// </summary>
public class Event
{
    /// <summary>
    /// Идентификатор мероприятия.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Название мероприятия.
    /// </summary>
    public string Title { get; private set; } = null!;

    /// <summary>
    /// Описание мероприятия.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Общее количество мест на событии
    /// </summary>
    public int TotalSeats { get; init; }

    /// <summary>
    /// Текущее количество свободных мест
    /// </summary>
    public int AvailableSeats { get; private set; }

    /// <summary>
    /// Дата и время начала мероприятия.
    /// </summary>
    public DateTime StartAt { get; private set; }

    /// <summary>
    /// Дата и время окончания мероприятия.
    /// </summary>
    public DateTime EndAt { get; private set; }

    public ICollection<Booking> Bookings { get; private set; } = [];

    private Event() { } // Для EF Core

    private Event(Guid id, string title, string? description, int totalSeats, DateTime startAt, DateTime endAt)
    {
        ValidateOnCreate(title, totalSeats, startAt, endAt);

        Id = id;
        Title = title;
        Description = description;
        TotalSeats = totalSeats;
        AvailableSeats = totalSeats;
        StartAt = startAt;
        EndAt = endAt;
    }


    /// <summary>
    /// Создает новое мероприятие с указанным количеством мест.
    /// </summary>
    /// <param name="title">Название мероприятия.</param>
    /// <param name="description">Описание мероприятия.</param>
    /// <param name="totalSeats">Общее количество мест.</param>
    /// <param name="startAt">Дата и время начала мероприятия.</param>
    /// <param name="endAt">Дата и время окончания мероприятия.</param>
    /// <returns>Созданное мероприятие.</returns>
    public static Event Create(string title, string? description, int totalSeats, DateTime startAt, DateTime endAt)
    {
        return new Event(Guid.NewGuid(), title, description, totalSeats, startAt, endAt);
    }


    /// <summary>
    /// Обновляет основные данные мероприятия без изменения количества мест.
    /// </summary>
    public void Update(string title, string? description, DateTime startAt, DateTime endAt)
    {
        ValidateTitle(title);
        ValidateDates(startAt, endAt);

        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
    }

    public bool TryReserveSeats(int count = 1)
    {
        if (count <= 0)
        {
            throw new ValidationException("Количество мест для бронирования должно быть больше нуля");
        }

        if (AvailableSeats < count)
        {
            return false;
        }

        AvailableSeats -= count;
        return true;
    }

    public void ReleaseSeats(int count = 1)
    {
        if (count <= 0)
        {
            throw new ValidationException("Количество освобождаемых мест должно быть больше нуля");
        }

        if (AvailableSeats + count > TotalSeats)
        {
            throw new ValidationException("Количество свободных мест не может быть больше общего количества мест");
        }

        AvailableSeats += count;
    }

    private static void ValidateOnCreate(string title, int totalSeats, DateTime startAt, DateTime endAt)
    {
        ValidateTitle(title);
        ValidateDates(startAt, endAt);

        if (totalSeats <= 0)
        {
            throw new ValidationException("Общее количество мест должно быть больше нуля");
        }
    }



    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ValidationException("Название мероприятия обязательно");
        }
    }

    private static void ValidateDates(DateTime startAt, DateTime endAt)
    {
        if (startAt >= endAt)
        {
            throw new ValidationException("Дата начала события должна быть раньше даты окончания");
        }
    }
}

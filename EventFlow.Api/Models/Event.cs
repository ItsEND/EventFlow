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
    public string Title { get; private set; }

    /// <summary>
    /// Описание мероприятия.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Дата и время начала мероприятия.
    /// </summary>
    public DateTime StartAt { get; private set; }

    /// <summary>
    /// Дата и время окончания мероприятия.
    /// </summary>
    public DateTime EndAt { get; private set; }

    private Event(Guid id, string title, string? description, DateTime startAt, DateTime endAt)
    {
        Validate(title, startAt, endAt);

        Id = id;
        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
    }

    public static Event Create(string title, string? description, DateTime startAt, DateTime endAt)
    {
        return new Event(Guid.NewGuid(), title, description, startAt, endAt);
    }

    public void Update(string title, string? description, DateTime startAt, DateTime endAt)
    {
        Validate(title, startAt, endAt);

        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
    }

    private void Validate(string title, DateTime startAt, DateTime endAt)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ValidationException("Название мероприятия обязательно");
        }

        if (startAt > endAt)
        {
            throw new ValidationException("Дата начала события не может быть позже даты окончания");
        }
    }
}

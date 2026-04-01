using EventFlow.Api.Models;
using EventFlow.Api.Services.Interfaces;

namespace EventFlow.Api.Services;

/// <summary>
/// Сервис для работы с мероприятиями, хранящимися в памяти приложения.
/// </summary>
public class EventService() : IEventService
{
    private List<Event> _events = [];

    /// <summary>
    /// Возвращает все мероприятия.
    /// </summary>
    /// <returns>Список всех мероприятий.</returns>
    public List<Event> GetEvents()
    {
        return _events;
    }

    /// <summary>
    /// Возвращает мероприятие по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор мероприятия.</param>
    /// <returns>Найденное мероприятие или null, если мероприятие не существует.</returns>
    public Event? GetEvent(Guid id)
    {
        return _events.FirstOrDefault(e => e.Id == id);
    }

    /// <summary>
    /// Создает новое мероприятие.
    /// </summary>
    /// <param name="newEvent">Данные для создания мероприятия.</param>
    /// <returns>Созданное мероприятие.</returns>
    public Event AddEvent(CreateEventModel newEvent)
    {
        var ev = new Event
        {
            Id = Guid.NewGuid(),
            Title = newEvent.Title,
            Description = newEvent.Description,
            StartAt = newEvent.StartAt,
            EndAt = newEvent.EndAt,
        };

        _events.Add(ev);

        return ev;
    }

    /// <summary>
    /// Обновляет существующее мероприятие.
    /// </summary>
    /// <param name="id">Идентификатор мероприятия.</param>
    /// <param name="updatedEvent">Новые данные мероприятия.</param>
    /// <returns>Обновленное мероприятие или null, если мероприятие не найдено.</returns>
    public Event? UpdateEvent(Guid id, UpdateEventModel updatedEvent)
    {
        var ev = GetEvent(id);

        if (ev is null)
        {
            return null;
        }

        ev.Title = updatedEvent.Title;
        ev.Description = updatedEvent.Description;
        ev.StartAt = updatedEvent.StartAt;
        ev.EndAt = updatedEvent.EndAt;

        return ev;
    }

    /// <summary>
    /// Удаляет мероприятие по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор мероприятия.</param>
    /// <returns>true, если мероприятие удалено; иначе false.</returns>
    public bool RemoveEvent(Guid id)
    {
        var ev = GetEvent(id);

        if (ev is null)
        {
            return false;
        }

        _events.Remove(ev);
        return true;
    }
}

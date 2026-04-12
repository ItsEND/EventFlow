using EventFlow.Api.Contracts;
using EventFlow.Api.Models;
using EventFlow.Api.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace EventFlow.Api.Services;

/// <summary>
/// Сервис для работы с мероприятиями, хранящимися в памяти приложения.
/// </summary>
public class EventService() : IEventService
{
    private List<Event> _events = [];

    public PaginatedResult<Event> GetEvents(int pageNumber, int pageSize, string? title = null, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        IEnumerable<Event> query = _events;

        if (!string.IsNullOrWhiteSpace(title))
        {
            query = query.Where(ev =>
            ev.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
        }

        if (dateFrom.HasValue)
        {
            query = query.Where(ev =>
            ev.StartAt >= dateFrom);
        }

        if (dateTo.HasValue)
        {
            query = query.Where(ev =>
            ev.EndAt <= dateTo);
        }

        var totalItems = query.Count();

        var items = query
            .OrderByDescending(ev => ev.StartAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        return new PaginatedResult<Event>(items, pageNumber, totalPages, totalItems);
    }

    /// <summary>
    /// Возвращает мероприятие по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор мероприятия.</param>
    /// <returns>Найденное мероприятие или null, если мероприятие не существует.</returns>
    public Event GetEvent(Guid id)
    {

        return _events.FirstOrDefault(e => e.Id == id)
            ?? throw new NotFoundException("Event", id);
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
    public Event UpdateEvent(Guid id, UpdateEventModel updatedEvent)
    {
        var ev = GetEvent(id);

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
    public void RemoveEvent(Guid id)
    {
        var ev = GetEvent(id);
        _events.Remove(ev);
    }
}

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
        ValidatePagination(pageNumber, pageSize);

        int totalItems;
        List<Event> items;

        BuildPaginateQuery(pageNumber, pageSize, title, dateFrom, dateTo, out totalItems, out items);

        var totalItemsOnPage = items.Count;
        var totalPages = totalItems == 0
            ? 0
            : (int)Math.Ceiling((double)totalItems / pageSize);

        return new PaginatedResult<Event>(
            items,
            pageNumber,
            pageSize,
            totalPages,
            totalItems,
            totalItemsOnPage);
    }

    private static void ValidatePagination(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            throw new ValidationException("Номер страницы должен быть больше или равен 1.");

        if (pageSize < 1)
            throw new ValidationException("Размер страницы должен быть больше или равен 1.");

        if (pageSize > 50)
            throw new ValidationException("Размер страницы не может быть больше 10.");
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
        var ev = Event.Create(

              newEvent.Title,
              newEvent.Description,
              newEvent.StartAt,
              newEvent.EndAt
          );

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

        ev.Update(
        updatedEvent.Title,
        updatedEvent.Description,
        updatedEvent.StartAt,
        updatedEvent.EndAt
        );
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

    private void BuildPaginateQuery(int pageNumber, int pageSize, string? title, DateTime? dateFrom, DateTime? dateTo,
        out int totalItems, out List<Event> items)
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

        totalItems = query.Count();
        items = query
            .OrderByDescending(ev => ev.StartAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }
}

using EventFlow.Api.Contracts;
using EventFlow.Api.Models;
using EventFlow.Api.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace EventFlow.Api.Services;

/// <summary>
/// Сервис для работы с мероприятиями, хранящимися в памяти приложения.
/// </summary>
public class EventService : IEventService
{
    private readonly List<Event> _events;

    /// <summary>
    /// Создает экземпляр сервиса мероприятий.
    /// </summary>
    /// <param name="initialEvents">Начальный список мероприятий (может быть null).</param>
    public EventService(IEnumerable<Event>? initialEvents)
    {
        _events = initialEvents?.ToList() ?? [];
    }

    /// <summary>
    /// Возвращает все отфильтрованные мероприятия.
    /// </summary>
    /// <param name="pageData">Данные для пагинации и фильтрации.</param>
    /// <returns>Результат отфильтрованного запроса с пагинацией.</returns>
    public PaginatedResult<Event> GetEvents(GetEventsQuery pageData)
    {
        ValidatePagination(pageData.PageNumber, pageData.PageSize);

        var query = ApplyFilters(pageData.Title, pageData.From, pageData.To);

        var totalItems = query.Count();
        var totalPages = (int)Math.Ceiling((double)totalItems / pageData.PageSize);

        var items = ApplyPaging(query, pageData.PageNumber, pageData.PageSize);

        var totalItemsOnPage = items.Count;

        return new PaginatedResult<Event>(
            items,
            pageData.PageNumber,
            pageData.PageSize,
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
            throw new ValidationException("Размер страницы не может быть больше 50.");
    }

    /// <summary>
    /// Возвращает мероприятие по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор мероприятия.</param>
    /// <returns>Найденное мероприятие.</returns>
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
    /// <returns>Обновленное мероприятие</returns>
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
    public void RemoveEvent(Guid id)
    {
        var ev = GetEvent(id);
        _events.Remove(ev);
    }




    private IEnumerable<Event> ApplyFilters(string? title, DateTime? dateFrom, DateTime? dateTo)
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
        return query;
    }
    private List<Event> ApplyPaging(IEnumerable<Event> query, int pageNumber, int pageSize)
    {
        return query
           .OrderByDescending(ev => ev.StartAt)
           .Skip((pageNumber - 1) * pageSize)
           .Take(pageSize)
           .ToList();
    }
}

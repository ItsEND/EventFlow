using EventFlow.Api.Contracts;
using EventFlow.Api.Contracts.Events;
using EventFlow.Api.Models;
using EventFlow.Api.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace EventFlow.Api.Services;

/// <summary>
/// Сервис для работы с мероприятиями, хранящимися в памяти приложения.
/// Выполняет операции создания, получения, обновления, удаления,
/// фильтрации и пагинации мероприятий.
/// </summary>
public class EventService : IEventService
{
    private readonly InMemoryEventStore _eventStore;

    /// <summary>
    /// Инициализирует новый экземпляр сервиса мероприятий.
    /// </summary>
    /// <param name="initialEvents">
    /// Начальный набор мероприятий. Если значение не передано,
    /// создаётся пустая коллекция.
    /// </param>
    public EventService(InMemoryEventStore eventStore)
    {
        _eventStore = eventStore;
    }


    /// <summary>
    /// Возвращает список мероприятий с учетом фильтрации и пагинации.
    /// </summary>
    /// <param name="pageData">Параметры фильтрации и пагинации.</param>
    /// <returns>Постраничный результат с мероприятиями.</returns>
    /// <exception cref="ValidationException">
    /// Выбрасывается, если параметры пагинации некорректны.
    /// </exception>
    public Task<PaginatedResult<Event>> GetEvents(GetEventsQuery pageData)
    {
        ValidatePagination(pageData.Page, pageData.PageSize);

        var filteredEvents = ApplyFilters(_eventStore.GetAll(), pageData.Title, pageData.From, pageData.To).ToList();

        var totalItems = filteredEvents.Count;
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageData.PageSize);

        var items = ApplyPaging(filteredEvents, pageData.Page, pageData.PageSize);
        var totalItemsOnPage = items.Count;

        var result = new PaginatedResult<Event>(
            items,
            pageData.Page,
            pageData.PageSize,
            totalPages,
            totalItems,
            totalItemsOnPage);

        return Task.FromResult(result);
    }


    /// <summary>
    /// Возвращает мероприятие по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор мероприятия.</param>
    /// <returns>Найденное мероприятие.</returns>
    /// <exception cref="NotFoundException">
    /// Выбрасывается, если мероприятие не найдено.
    /// </exception>
    public Task<Event> GetEvent(Guid id)
    {
        var ev = _eventStore.Get(id);

        return Task.FromResult(ev);
    }

    /// <summary>
    /// Создает новое мероприятие.
    /// </summary>
    /// <param name="newEvent">Данные для создания мероприятия.</param>
    /// <returns>Созданное мероприятие.</returns>
    public Task<Event> CreateEventAsync(CreateEventModel newEvent)
    {
        var createdEvent = Event.Create(
            newEvent.Title,
            newEvent.Description,
            newEvent.TotalSeats,
            newEvent.StartAt,
            newEvent.EndAt);

        _eventStore.Add(createdEvent);

        return Task.FromResult(createdEvent);
    }

    /// <summary>
    /// Обновляет существующее мероприятие.
    /// </summary>
    /// <param name="id">Идентификатор мероприятия.</param>
    /// <param name="updatedEvent">Новые данные мероприятия.</param>
    /// <returns>Обновленное мероприятие.</returns>
    /// <exception cref="NotFoundException">
    /// Выбрасывается, если мероприятие не найдено.
    /// </exception>
    public Task<Event> UpdateEvent(Guid id, UpdateEventModel updatedEvent)
    {
        var existingEvent = _eventStore.Get(id);

        existingEvent.Update(
            updatedEvent.Title,
            updatedEvent.Description,
            updatedEvent.StartAt,
            updatedEvent.EndAt);

        return Task.FromResult(existingEvent);
    }

    /// <summary>
    /// Удаляет мероприятие по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор мероприятия.</param>
    /// <exception cref="NotFoundException">
    /// Выбрасывается, если мероприятие не найдено.
    /// </exception>
    public Task RemoveEvent(Guid id)
    {
        _eventStore.Remove(id);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Проверяет корректность параметров пагинации.
    /// </summary>
    /// <param name="page">Номер страницы.</param>
    /// <param name="pageSize">Размер страницы.</param>
    /// <exception cref="ValidationException">
    /// Выбрасывается, если номер страницы или размер страницы некорректны.
    /// </exception>
    private static void ValidatePagination(int page, int pageSize)
    {
        if (page < 1)
        {
            throw new ValidationException("Номер страницы должен быть больше или равен 1.");
        }

        if (pageSize < 1)
        {
            throw new ValidationException("Размер страницы должен быть больше или равен 1.");
        }

        if (pageSize > 50)
        {
            throw new ValidationException("Размер страницы не может быть больше 50.");
        }
    }

    /// <summary>
    /// Применяет фильтрацию к списку мероприятий по названию и диапазону дат.
    /// </summary>
    /// <param name="title">Фильтр по названию мероприятия.</param>
    /// <param name="dateFrom">Нижняя граница даты начала.</param>
    /// <param name="dateTo">Верхняя граница даты окончания.</param>
    /// <returns>Отфильтрованная последовательность мероприятий.</returns>
    private IEnumerable<Event> ApplyFilters(IEnumerable<Event> events, string? title, DateTime? dateFrom, DateTime? dateTo)
    {
        var query = events;

        if (!string.IsNullOrWhiteSpace(title))
        {
            query = query.Where(e =>
                e.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
        }

        if (dateFrom.HasValue)
        {
            query = query.Where(e => e.StartAt >= dateFrom.Value);
        }

        if (dateTo.HasValue)
        {
            query = query.Where(e => e.EndAt <= dateTo.Value);
        }

        return query;
    }

    /// <summary>
    /// Применяет пагинацию к последовательности мероприятий.
    /// </summary>
    /// <param name="query">Исходная последовательность мероприятий.</param>
    /// <param name="page">Номер страницы.</param>
    /// <param name="pageSize">Размер страницы.</param>
    /// <returns>Список мероприятий для указанной страницы.</returns>
    private static List<Event> ApplyPaging(IEnumerable<Event> query, int page, int pageSize)
    {
        return query
            .OrderByDescending(e => e.StartAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }
}
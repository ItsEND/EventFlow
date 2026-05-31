using EventFlow.Api.Contracts;
using EventFlow.Api.Contracts.Events;
using EventFlow.Api.DataAccess;
using EventFlow.Api.Models;
using EventFlow.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EventFlow.Api.Services;

/// <summary>
/// Сервис для работы с мероприятиями, хранящимися в памяти приложения.
/// Выполняет операции создания, получения, обновления, удаления,
/// фильтрации и пагинации мероприятий.
/// </summary>
public class EventService : IEventService
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Инициализирует новый экземпляр сервиса мероприятий.
    /// </summary>
    /// <param name="context">
    /// Контекст базы данных.
    /// </param>
    public EventService(AppDbContext context)
    {
        _context = context;
    }


    /// <summary>
    /// Возвращает список мероприятий с учетом фильтрации и пагинации.
    /// </summary>
    /// <param name="pageData">Параметры фильтрации и пагинации.</param>
    /// <returns>Постраничный результат с мероприятиями.</returns>
    /// <exception cref="ValidationException">
    /// Выбрасывается, если параметры пагинации некорректны.
    /// </exception>
    public async Task<PaginatedResult<Event>> GetEvents(GetEventsQuery pageData, CancellationToken ct = default)
    {
        ValidatePagination(pageData.Page, pageData.PageSize);

        var filteredEvents = ApplyFilters(pageData.Title, pageData.From, pageData.To);

        var totalItems = await filteredEvents.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageData.PageSize);

        var items = await ApplyPaging(filteredEvents, pageData.Page, pageData.PageSize);
        var totalItemsOnPage = items.Count;

        return new PaginatedResult<Event>(
            items,
            pageData.Page,
            pageData.PageSize,
            totalPages,
            totalItems,
            totalItemsOnPage);
    }


    /// <summary>
    /// Возвращает мероприятие по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор мероприятия.</param>
    /// <returns>Найденное мероприятие.</returns>
    /// <exception cref="NotFoundException">
    /// Выбрасывается, если мероприятие не найдено.
    /// </exception>
    public async Task<Event> GetEventAsync(Guid id, CancellationToken ct)
    {
        return await _context.Events.FindAsync([id], ct)
            ?? throw new NotFoundException("Event", id);
    }

    /// <summary>
    /// Создает новое мероприятие.
    /// </summary>
    /// <param name="newEvent">Данные для создания мероприятия.</param>
    /// <returns>Созданное мероприятие.</returns>
    public async Task<Event> CreateEventAsync(CreateEventModel newEvent, CancellationToken ct)
    {
        var createdEvent = Event.Create(
            newEvent.Title,
            newEvent.Description,
            newEvent.TotalSeats,
            newEvent.StartAt,
            newEvent.EndAt);

        _context.Events.Add(createdEvent);

        await _context.SaveChangesAsync(ct);
        return createdEvent;
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
    public async Task<Event> UpdateEventAsync(Guid id, UpdateEventModel updatedEvent, CancellationToken ct)
    {
        var existingEvent = await GetEventAsync(id, ct);

        existingEvent.Update(
            updatedEvent.Title,
            updatedEvent.Description,
            updatedEvent.StartAt,
            updatedEvent.EndAt);

        await _context.SaveChangesAsync(ct);
        return existingEvent;
    }

    /// <summary>
    /// Удаляет мероприятие по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор мероприятия.</param>
    /// <exception cref="NotFoundException">
    /// Выбрасывается, если мероприятие не найдено.
    /// </exception>
    public async Task RemoveEventAsync(Guid id, CancellationToken ct)
    {
        var ev = await GetEventAsync(id, ct);
        _context.Events.Remove(ev);
        await _context.SaveChangesAsync(ct);
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
    private IQueryable<Event> ApplyFilters(string? title, DateTime? dateFrom, DateTime? dateTo)
    {
        var query = _context.Events.AsNoTracking();

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
    private static Task<List<Event>> ApplyPaging(IQueryable<Event> query, int page, int pageSize)
    {
        return query
            .OrderByDescending(e => e.StartAt)
                .ThenBy(e => e.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}
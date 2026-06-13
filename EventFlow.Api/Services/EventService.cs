using EventFlow.Api.Contracts;
using EventFlow.Api.Contracts.Events;
using EventFlow.Api.Models;
using EventFlow.Api.Repositories.Interfaces;
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
    private readonly IEventRepository _eventRepository;
    /// <summary>
    /// Инициализирует новый экземпляр сервиса мероприятий.
    /// </summary>
    /// <param name="eventRepository">
    /// Репозиторий базы данных.
    /// </param>
    public EventService(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }


    /// <summary>
    /// Возвращает список мероприятий с учетом фильтрации и пагинации.
    /// </summary>
    /// <param name="pageData">Параметры фильтрации и пагинации.</param>
    /// <returns>Постраничный результат с мероприятиями.</returns>
    /// <exception cref="ValidationException">
    /// Выбрасывается, если параметры пагинации некорректны.
    /// </exception>
    public async Task<PaginatedResult<Event>> GetEventsAsync(GetEventsQuery pageData, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(pageData);
        ValidatePagination(pageData.Page, pageData.PageSize);

        var result = await _eventRepository.GetPageAsync(pageData.Title, pageData.From, pageData.To, pageData.Page, pageData.PageSize, ct);

        var totalPages = (int)Math.Ceiling(result.TotalItems / (double)pageData.PageSize);


        return new PaginatedResult<Event>(
            result.Items,
            pageData.Page,
            pageData.PageSize,
            totalPages,
            result.TotalItems,
            result.Items.Count);
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
        return await _eventRepository.GetByIdAsync(id, ct)
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

        await _eventRepository.AddAsync(createdEvent, ct);

        await _eventRepository.SaveChangesAsync(ct);
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

        await _eventRepository.SaveChangesAsync(ct);
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
        _eventRepository.Remove(ev, ct);
        await _eventRepository.SaveChangesAsync(ct);

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
}
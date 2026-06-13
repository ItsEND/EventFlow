using EventFlow.Api.Contracts;
using EventFlow.Api.Contracts.Events;
using EventFlow.Api.Models;

namespace EventFlow.Api.Services.Interfaces;

/// <summary>
/// Определяет контракт сервиса для управления мероприятиями:
/// получения, создания, обновления, удаления,
/// а также фильтрации и пагинации списка мероприятий.
/// </summary>
public interface IEventService
{
    /// <summary>
    /// Возвращает список мероприятий с учетом параметров фильтрации и пагинации.
    /// </summary>
    /// <param name="pageData">Параметры фильтрации и пагинации.</param>
    /// <returns>Постраничный результат с мероприятиями.</returns>
    Task<PaginatedResult<Event>> GetEventsAsync(GetEventsQuery pageData, CancellationToken ct = default);

    /// <summary>
    /// Возвращает мероприятие по его идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор мероприятия.</param>
    /// <returns>Найденное мероприятие.</returns>
    /// <exception cref="NotFoundException">
    /// Выбрасывается, если мероприятие с указанным идентификатором не найдено.
    /// </exception>
    Task<Event> GetEventAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Создает новое мероприятие.
    /// </summary>
    /// <param name="newEvent">Данные для создания мероприятия.</param>
    /// <returns>Созданное мероприятие.</returns>
    Task<Event> CreateEventAsync(CreateEventModel newEvent, CancellationToken ct = default);

    /// <summary>
    /// Обновляет существующее мероприятие.
    /// </summary>
    /// <param name="id">Идентификатор мероприятия.</param>
    /// <param name="updatedEvent">Новые данные мероприятия.</param>
    /// <returns>Обновленное мероприятие.</returns>
    /// <exception cref="NotFoundException">
    /// Выбрасывается, если мероприятие с указанным идентификатором не найдено.
    /// </exception>
    Task<Event> UpdateEventAsync(Guid id, UpdateEventModel updatedEvent, CancellationToken ct = default);

    /// <summary>
    /// Удаляет мероприятие по его идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор мероприятия.</param>
    /// <exception cref="NotFoundException">
    /// Выбрасывается, если мероприятие с указанным идентификатором не найдено.
    /// </exception>
    Task RemoveEventAsync(Guid id, CancellationToken ct = default);
}
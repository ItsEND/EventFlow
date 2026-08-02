using EventFlow.Events.Application.Dtos;
using EventFlow.Events.Application.Dtos.Events;

namespace EventFlow.Events.Application.Abstractions.Services;

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
    Task<PaginatedResult<EventDto>> GetEventsAsync(GetEventsQuery pageData, CancellationToken ct = default);

    /// <summary>
    /// Возвращает мероприятие по его идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор мероприятия.</param>
    /// <returns>Найденное мероприятие.</returns>
    Task<EventDto> GetEventAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Создает новое мероприятие.
    /// </summary>
    /// <param name="newEvent">Данные для создания мероприятия.</param>
    /// <returns>Созданное мероприятие.</returns>
    Task<EventDto> CreateEventAsync(CreateEventModel newEvent, CancellationToken ct = default);

    /// <summary>
    /// Обновляет существующее мероприятие.
    /// </summary>
    /// <param name="id">Идентификатор мероприятия.</param>
    /// <param name="updatedEvent">Новые данные мероприятия.</param>
    /// <returns>Обновленное мероприятие.</returns>
    Task<EventDto> UpdateEventAsync(Guid id, UpdateEventModel updatedEvent, CancellationToken ct = default);

    /// <summary>
    /// Удаляет мероприятие по его идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор мероприятия.</param>
    Task RemoveEventAsync(Guid id, CancellationToken ct = default);
}

using EventFlow.Api.Contracts;
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
    PaginatedResult<Event> GetEvents(GetEventsQuery pageData);

    /// <summary>
    /// Возвращает мероприятие по его идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор мероприятия.</param>
    /// <returns>Найденное мероприятие.</returns>
    /// <exception cref="NotFoundException">
    /// Выбрасывается, если мероприятие с указанным идентификатором не найдено.
    /// </exception>
    Event GetEvent(Guid id);

    /// <summary>
    /// Создает новое мероприятие.
    /// </summary>
    /// <param name="newEvent">Данные для создания мероприятия.</param>
    /// <returns>Созданное мероприятие.</returns>
    Event AddEvent(CreateEventModel newEvent);

    /// <summary>
    /// Обновляет существующее мероприятие.
    /// </summary>
    /// <param name="id">Идентификатор мероприятия.</param>
    /// <param name="updatedEvent">Новые данные мероприятия.</param>
    /// <returns>Обновленное мероприятие.</returns>
    /// <exception cref="NotFoundException">
    /// Выбрасывается, если мероприятие с указанным идентификатором не найдено.
    /// </exception>
    Event UpdateEvent(Guid id, UpdateEventModel updatedEvent);

    /// <summary>
    /// Удаляет мероприятие по его идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор мероприятия.</param>
    /// <exception cref="NotFoundException">
    /// Выбрасывается, если мероприятие с указанным идентификатором не найдено.
    /// </exception>
    void RemoveEvent(Guid id);
}
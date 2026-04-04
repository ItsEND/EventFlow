using EventFlow.Api.Models;
namespace EventFlow.Api.Services.Interfaces;
/// <summary>
/// Предоставляет методы для управления мероприятиями.
/// </summary>
public interface IEventService
{
    /// <summary>
    /// Возвращает все мероприятия.
    /// </summary>
    /// <returns>Список всех мероприятий.</returns>
    List<Event> GetEvents();

    /// <summary>
    /// Возвращает мероприятие по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор мероприятия.</param>
    /// <returns>Найденное мероприятие или null, если мероприятие не существует.</returns>
    Event? GetEvent(Guid id);

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
    /// <returns>Обновленное мероприятие или null, если мероприятие не найдено.</returns>
    Event? UpdateEvent(Guid id, UpdateEventModel updatedEvent);

    /// <summary>
    /// Удаляет мероприятие по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор мероприятия.</param>
    /// <returns>true, если мероприятие удалено; иначе false.</returns>
    bool RemoveEvent(Guid id);
}

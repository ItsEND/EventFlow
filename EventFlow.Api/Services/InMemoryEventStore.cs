using EventFlow.Api.Models;
using System.Collections.Concurrent;

namespace EventFlow.Api.Services;

/// <summary>
/// Потокобезопасное in-memory-хранилище мероприятий.
/// Используется для хранения, получения, добавления и удаления событий
/// без подключения к внешней базе данных.
/// </summary>
public class InMemoryEventStore
{
    private readonly ConcurrentDictionary<Guid, Event> _events = new();

    /// <summary>
    /// Инициализирует хранилище мероприятий.
    /// При наличии начального набора событий добавляет их в коллекцию.
    /// </summary>
    /// <param name="initialEvents">Начальный набор мероприятий.</param>
    public InMemoryEventStore(IEnumerable<Event>? initialEvents = null)
    {
        if (initialEvents is null)
        {
            return;
        }

        foreach (var ev in initialEvents)
        {
            _events.TryAdd(ev.Id, ev);
        }
    }

    /// <summary>
    /// Возвращает все мероприятия из хранилища.
    /// </summary>
    /// <returns>Коллекция всех мероприятий.</returns>
    public IReadOnlyCollection<Event> GetAll()
    {
        return _events.Values.ToList();
    }

    /// <summary>
    /// Возвращает мероприятие по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор мероприятия.</param>
    /// <returns>Найденное мероприятие.</returns>
    /// <exception cref="NotFoundException">
    /// Выбрасывается, если мероприятие с указанным идентификатором не найдено.
    /// </exception>

    public Event Get(Guid id)
    {
        return _events.TryGetValue(id, out var ev)
            ? ev
            : throw new NotFoundException("Event", id);
    }

    /// <summary>
    /// Пытается получить мероприятие по идентификатору без выбрасывания исключения.
    /// </summary>
    /// <param name="id">Идентификатор мероприятия.</param>
    /// <param name="ev">Найденное мероприятие, если оно существует.</param>
    /// <returns>
    /// true, если мероприятие найдено; иначе false.
    /// </returns>
    public bool TryGet(Guid id, out Event? ev)
    {
        return _events.TryGetValue(id, out ev);
    }

    /// <summary>
    /// Добавляет новое мероприятие в хранилище.
    /// </summary>
    /// <param name="ev">Мероприятие для добавления.</param>
    /// <exception cref="InvalidOperationException">
    /// Выбрасывается, если мероприятие с таким идентификатором уже существует.
    /// </exception>
    public void Add(Event ev)
    {
        if (!_events.TryAdd(ev.Id, ev))
        {
            throw new InvalidOperationException($"Событие с id {ev.Id} уже существует.");
        }
    }

    /// <summary>
    /// Удаляет мероприятие из хранилища по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор мероприятия.</param>
    /// <exception cref="NotFoundException">
    /// Выбрасывается, если мероприятие с указанным идентификатором не найдено.
    /// </exception>
    public void Remove(Guid id)
    {
        if (!_events.TryRemove(id, out _))
        {
            throw new NotFoundException("Event", id);
        }
    }
}

using EventFlow.Api.Models;
using EventFlow.Api.Services.Interfaces;

namespace EventFlow.Api.Services;

public class EventService() : IEventService
{
    private List<Event> _events = [];

    public List<Event> GetEvents()
    {
        return _events;
    }

    public Event? GetEvent(Guid id)
    {
        return _events.FirstOrDefault(e => e.Id == id);
    }

    public Event AddEvent(CreateEventModel newEvent)
    {
        var ev = new Event
        {
            Id = Guid.NewGuid(),
            Title = newEvent.Title,
            Description = newEvent.Description,
            StartAt = newEvent.StartAt,
            EndAt = newEvent.EndAt,
        };

        _events.Add(ev);

        return ev;
    }

    public Event? UpdateEvent(Guid id, UpdateEventModel updatedEvent)
    {
        var ev = GetEvent(id);

        if (ev is null)
        {
            return null;
        }

        ev.Title = updatedEvent.Title;
        ev.Description = updatedEvent.Description;
        ev.StartAt = updatedEvent.StartAt;
        ev.EndAt = updatedEvent.EndAt;

        return ev;
    }

    public bool RemoveEvent(Guid id)
    {
        var ev = GetEvent(id);

        if (ev is null)
        {
            return false;
        }

        _events.Remove(ev);
        return true;
    }
}

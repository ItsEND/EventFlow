using EventFlow.Api.Contracts;
using EventFlow.Api.Models;
using EventFlow.Api.Services.Interfaces;

namespace EventFlow.Api.Services;

public class EventService() : IEventService
{
    public static List<Event> Events { get; set; } = [];

    public List<Event> GetEvents()
    {
        return Events;
    }

    public Event GetEvent(Guid id)
    {
        var ev = Events.FirstOrDefault(e => e.Id == id);
        return ev;
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

        Events.Add(ev);

        return ev;
    }

    public Event ChangeEvent(Guid id, UpdateEventRequest newEvent)
    {
        var ev = GetEvent(id);

        if (ev != null)
        {
            ev.Title = newEvent.Title;
            ev?.Description = newEvent.Description;
            ev.StartAt = newEvent.StartAt;
            ev.EndAt = newEvent.EndAt;

            return ev;
        }
        return null;
    }

    public bool RemoveEvent(Guid id)
    {
        var ev = GetEvent(id);

        if (ev != null)
        {
            Events.Remove(ev);
            return true;
        }
        return false;
    }
}

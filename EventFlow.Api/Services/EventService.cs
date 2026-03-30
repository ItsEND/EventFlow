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

    public Event AddEvent(Event newEvent)
    {
        Events.Add(newEvent);

        return newEvent;
    }

    public bool ChangeEvent(Guid id, Event newEvent)
    {
        var ev = GetEvent(id);

        if (ev != null)
        {
            ev.Title = newEvent.Title;
            ev?.Description = newEvent.Description;
            ev.StartAt = newEvent.StartAt;
            ev.EndAt = newEvent.EndAt;

            return true;
        }
        return false;
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

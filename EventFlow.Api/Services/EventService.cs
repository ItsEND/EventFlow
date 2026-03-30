using EventFlow.Api.Models;
using EventFlow.Api.Contracts;
using EventFlow.Api.Services.Interfaces;
namespace EventFlow.Api.Services;

public class EventService() : IEventService
{
    public static List<Event> Events { get; set; } = [];

    public List<EventResponse> GetEvents()
    {
        List<EventResponse> responses = new List<EventResponse>();

        if (Events != null)
        {
            foreach (var ev in Events)
            {
                responses.Add(
                new EventResponse
                {
                    Id = ev.Id,
                    Title = ev.Title,
                    Description = ev.Description,
                    StartAt = ev.StartAt,
                    EndAt = ev.EndAt,
                });

            }

        }
        return responses;


    }
    public EventResponse GetEvent(Guid id)
    {
        var ev = Events.FirstOrDefault(e => e.Id == id);

        if (ev == null)
        {
            return null;
        }

        var response = CreateEventResponse(ev);
        return response;
    }

    public EventResponse AddEvent(EventRequest newEvent)
    {
        var ev = CreateEvent(newEvent);
        Events.Add(ev);

        var response = CreateEventResponse(ev);

        return response;

    }

    public bool ChangeEvent(Guid id, UpdateEventRequest newEvent)
    {
        var ev = GetEventById(id);

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
        var ev = GetEventById(id);

        if (ev != null)
        {
            Events.Remove(ev);
            return true;
        }

        return false;
    }

    private Event GetEventById(Guid id)
    {
        var ev = Events.FirstOrDefault(e => e.Id == id);

        if (ev == null)
        {
            return null;
        }

        return ev;
    }
    
    private EventResponse CreateEventResponse(Event ev)
    {
        return new EventResponse
        {
            Id = ev.Id,
            Title = ev.Title,
            Description = ev.Description,
            StartAt = ev.StartAt,
            EndAt = ev.EndAt,
        };
    }

    private Event CreateEvent(EventRequest request)
    {
        return new Event
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
        };
    }
    
}

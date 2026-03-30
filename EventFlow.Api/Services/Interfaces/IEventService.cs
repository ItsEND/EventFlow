using EventFlow.Api.Contracts;
using EventFlow.Api.Models;

namespace EventFlow.Api.Services.Interfaces;

public interface IEventService
{
    List<EventResponse> GetEvents();
    EventResponse GetEvent(Guid id);
    EventResponse AddEvent(EventRequest newEvent);
    bool ChangeEvent(Guid id, UpdateEventRequest newEvent);
    bool RemoveEvent(Guid id);
}

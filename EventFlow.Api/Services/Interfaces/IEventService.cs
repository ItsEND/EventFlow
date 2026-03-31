using EventFlow.Api.Models;
namespace EventFlow.Api.Services.Interfaces;

public interface IEventService
{
    List<Event> GetEvents();
    Event? GetEvent(Guid id);
    Event AddEvent(CreateEventModel newEvent);
    Event? UpdateEvent(Guid id, UpdateEventModel updatedEvent);
    bool RemoveEvent(Guid id);
}

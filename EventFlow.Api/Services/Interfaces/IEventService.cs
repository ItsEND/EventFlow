using EventFlow.Api.Contracts;
using EventFlow.Api.Models;
namespace EventFlow.Api.Services.Interfaces;

public interface IEventService
{
    List<Event> GetEvents();
    Event GetEvent(Guid id);
    Event AddEvent(CreateEventModel newEvent);
    Event ChangeEvent(Guid id, UpdateEventRequest newEvent);
    bool RemoveEvent(Guid id);
}

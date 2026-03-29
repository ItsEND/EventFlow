using EventFlow.Api.Models;

namespace EventFlow.Api.Services.Interfaces;

public interface IEventService
{
    List<Event> GetEvents();
    Event GetEvent(Guid id);
    void AddEvent(Event newEvent);
    bool ChangeEvent(Guid id, Event newEvent);
    bool RemoveEvent(Guid id);
}

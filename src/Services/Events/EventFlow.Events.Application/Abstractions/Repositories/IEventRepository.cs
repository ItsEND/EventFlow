using EventFlow.Events.Domain.Models;
using EventFlow.Events.Application.Contracts.Events;

namespace EventFlow.Events.Application.Abstractions.Repositories;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EventPage> GetPageAsync(GetEventsQuery query, CancellationToken cancellationToken = default);
    void Add(Event ev);
    void Remove(Event ev);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

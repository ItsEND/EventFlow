using EventFlow.Api.Models;
namespace EventFlow.Api.Repositories.Interfaces;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EventPage> GetPageAsync(string? title, DateTime? dateFrom, DateTime? dateTo, int page, int pageSize, CancellationToken cancellationToken = default);
    void Add(Event ev);
    void Remove(Event ev, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

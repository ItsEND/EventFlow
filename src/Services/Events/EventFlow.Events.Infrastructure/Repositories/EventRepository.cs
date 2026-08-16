using EventFlow.Events.Application.Abstractions.Repositories;
using EventFlow.Events.Application.Contracts.Events;
using EventFlow.Events.Domain.Models;
using EventFlow.Events.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Events.Infrastructure.Repositories;

public class EventRepository(EventDbContext db) : IEventRepository
{
    public Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => db.Events.FirstOrDefaultAsync(ev => ev.Id == id, cancellationToken);

    public async Task<EventPage> GetPageAsync(GetEventsQuery query, CancellationToken cancellationToken = default)
    {
        var events = db.Events.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Title))
        {
            var normalizedTitle = query.Title.Trim().ToLower();
            events = events.Where(e => e.Title.ToLower().Contains(normalizedTitle));
        }

        if (query.From.HasValue)
        {
            events = events.Where(e => e.StartAt >= query.From.Value);
        }

        if (query.To.HasValue)
        {
            events = events.Where(e => e.EndAt <= query.To.Value);
        }

        var totalItems = await events.CountAsync(cancellationToken);

        var items = await events
            .OrderByDescending(e => e.StartAt)
            .ThenBy(e => e.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new EventPage(items, totalItems);
    }

    public void Remove(Event ev)
    {
        db.Events.Remove(ev);
    }

    public void Add(Event ev)
    {
        db.Events.Add(ev);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => db.SaveChangesAsync(cancellationToken);

    public async Task<IReadOnlyList<Event>> GetTopEventsAsync(int limit, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit);


        return await db.Events.AsNoTracking()
            .OrderByDescending(ev => (decimal) (ev.TotalSeats - ev.AvailableSeats) / ev.TotalSeats)
            .ThenByDescending(ev => ev.TotalSeats - ev.AvailableSeats)
            .ThenBy(ev => ev.Id)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}

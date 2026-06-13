using EventFlow.Api.DataAccess;
using EventFlow.Api.Models;
using EventFlow.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Api.Repositories;

public class EventRepository : IEventRepository
{
    private readonly AppDbContext _db;

    public EventRepository(AppDbContext db)
    {
        _db = db;
    }
    public Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _db.Events.FirstOrDefaultAsync(ev => ev.Id == id, cancellationToken);


    public async Task<EventPage> GetPageAsync(string? title, DateTime? dateFrom, DateTime? dateTo, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _db.Events.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(title))
        {
            var normalizedTitle = title.Trim().ToLower();
            query = query.Where(e => e.Title.ToLower().Contains(normalizedTitle));
        }

        if (dateFrom.HasValue)
        {
            query = query.Where(e => e.StartAt >= dateFrom.Value);
        }

        if (dateTo.HasValue)
        {
            query = query.Where(e => e.EndAt <= dateTo.Value);
        }
        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
           .OrderByDescending(e => e.StartAt)
           .ThenBy(e => e.Id)
           .Skip((page - 1) * pageSize)
           .Take(pageSize)
           .ToListAsync(cancellationToken);

        return new EventPage(items, totalItems);
    }

    public void Remove(Event ev, CancellationToken cancellationToken = default)
    {
        _db.Events.Remove(ev);
    }

    public Task AddAsync(Event ev, CancellationToken cancellationToken = default)
        => _db.Events.AddAsync(ev, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _db.SaveChangesAsync(cancellationToken);

}

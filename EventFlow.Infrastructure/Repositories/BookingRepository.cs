using EventFlow.Application.Abstractions.Repositories;
using EventFlow.Domain.Models;
using EventFlow.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Infrastructure.Repositories;

public class BookingRepository(AppDbContext db) : IBookingRepository
{
    public void Add(Booking booking)
    {
        db.Bookings.Add(booking);
    }

    public Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => db.Bookings.FirstOrDefaultAsync(book => book.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Guid>> GetPendingIdsAsync(CancellationToken cancellationToken = default)
    {
        return await db.Bookings.AsNoTracking()
            .Where(booking => booking.Status == BookingStatus.Pending)
            .Select(booking => booking.Id)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => db.SaveChangesAsync(cancellationToken);
}

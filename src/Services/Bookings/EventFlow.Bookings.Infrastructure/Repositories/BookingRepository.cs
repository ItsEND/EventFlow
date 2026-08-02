using EventFlow.Bookings.Application.Abstractions.Repositories;
using EventFlow.Bookings.Domain.Models;
using EventFlow.Bookings.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Bookings.Infrastructure.Repositories;

public class BookingRepository(AppDbContext db) : IBookingRepository
{
    public void Add(Booking booking)
    {
        db.Bookings.Add(booking);
    }

    public Task<int> CountActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return db.Bookings
        .CountAsync(booking => booking.UserId == userId && 
        (booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.Confirmed),
        cancellationToken);
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

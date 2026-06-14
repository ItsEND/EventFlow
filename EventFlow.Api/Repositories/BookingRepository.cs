using EventFlow.Api.DataAccess;
using EventFlow.Api.Models;
using EventFlow.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Api.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _db;

        public BookingRepository(AppDbContext db)
        {
            _db = db;
        }
        public void Add(Booking booking) => _db.Bookings.Add(booking);
        public Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            _db.Bookings.FirstOrDefaultAsync(book => book.Id == id, cancellationToken);

        public async Task<IReadOnlyList<Guid>> GetPendingIdsAsync(CancellationToken cancellationToken = default)
        {
            return await _db.Bookings.AsNoTracking()
                .Where(booking => booking.Status == BookingStatus.Pending)
                .Select(bookong => bookong.Id)
                .ToListAsync(cancellationToken);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => _db.SaveChangesAsync(cancellationToken);
    }
}

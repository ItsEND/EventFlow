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
        public Task AddAsync(Booking booking, CancellationToken cancellationToken = default)
            => _db.Bookings.AddAsync(booking, cancellationToken).AsTask();


        public Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            _db.Bookings.FirstOrDefaultAsync(book => book.Id == id, cancellationToken);


        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => _db.SaveChangesAsync(cancellationToken);

    }
}

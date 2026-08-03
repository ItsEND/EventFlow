using EventFlow.Bookings.Domain.Models;
using EventFlow.Bookings.Infrastructure.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Bookings.Infrastructure.DataAccess;

public sealed class BookingDbContext(DbContextOptions<BookingDbContext> options) : DbContext(options)
{
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookingDbContext).Assembly);
    }
}

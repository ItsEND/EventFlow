using EventFlow.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Api.DataAccess;

public sealed class AppDbContext : DbContext
{
    public DbSet<Event> Events  => Set<Event>();
    public DbSet<Booking> Bookings => Set<Booking>();   

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

}

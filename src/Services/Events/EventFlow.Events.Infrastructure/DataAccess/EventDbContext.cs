using EventFlow.Events.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Events.Infrastructure.DataAccess;

public sealed class EventDbContext(DbContextOptions<EventDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events => Set<Event>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventDbContext).Assembly);
    }
}

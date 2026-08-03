using EventFlow.Events.Domain.Models;
using EventFlow.Events.Infrastructure.Messaging.Inbox;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Events.Infrastructure.DataAccess;

public sealed class EventDbContext(DbContextOptions<EventDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events => Set<Event>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventDbContext).Assembly);
    }
}

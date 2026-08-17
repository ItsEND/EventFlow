using Microsoft.EntityFrameworkCore;
using PlatformTemplate.ServiceName.Domain.Models;
#if (UseOutbox)
using PlatformTemplate.ServiceName.Infrastructure.Messaging.Outbox;
#endif
#if (UseInbox)
using PlatformTemplate.ServiceName.Infrastructure.Messaging.Inbox;
#endif

namespace PlatformTemplate.ServiceName.Infrastructure.DataAccess;

public sealed class ServiceNameDbContext(DbContextOptions<ServiceNameDbContext> options)
    : DbContext(options)
{
    public DbSet<ServiceNameItem> Items => Set<ServiceNameItem>();

#if (UseOutbox)
    internal DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
#endif
#if (UseInbox)
    internal DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
#endif

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ServiceName");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ServiceNameDbContext).Assembly);
    }
}

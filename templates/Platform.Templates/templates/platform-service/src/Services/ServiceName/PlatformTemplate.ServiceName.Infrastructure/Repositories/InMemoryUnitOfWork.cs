using PlatformTemplate.ServiceName.Application.Abstractions.Persistence;

namespace PlatformTemplate.ServiceName.Infrastructure.Repositories;

internal sealed class InMemoryUnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(0);
    }
}

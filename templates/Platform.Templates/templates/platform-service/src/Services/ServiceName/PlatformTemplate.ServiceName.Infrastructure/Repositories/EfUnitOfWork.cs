using PlatformTemplate.ServiceName.Application.Abstractions.Persistence;
using PlatformTemplate.ServiceName.Infrastructure.DataAccess;

namespace PlatformTemplate.ServiceName.Infrastructure.Repositories;

internal sealed class EfUnitOfWork(ServiceNameDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}

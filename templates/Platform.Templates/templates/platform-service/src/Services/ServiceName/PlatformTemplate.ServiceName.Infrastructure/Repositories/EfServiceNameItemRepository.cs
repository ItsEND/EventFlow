using Microsoft.EntityFrameworkCore;
using PlatformTemplate.ServiceName.Application.Abstractions.Persistence;
using PlatformTemplate.ServiceName.Domain.Models;
using PlatformTemplate.ServiceName.Infrastructure.DataAccess;

namespace PlatformTemplate.ServiceName.Infrastructure.Repositories;

internal sealed class EfServiceNameItemRepository(ServiceNameDbContext dbContext)
    : IServiceNameItemRepository
{
    public Task<ServiceNameItem?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Items.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

    public void Add(ServiceNameItem item) => dbContext.Items.Add(item);
}

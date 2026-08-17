using PlatformTemplate.ServiceName.Domain.Models;

namespace PlatformTemplate.ServiceName.Application.Abstractions.Persistence;

public interface IServiceNameItemRepository
{
    Task<ServiceNameItem?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    void Add(ServiceNameItem item);
}

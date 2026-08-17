using PlatformTemplate.ServiceName.Application.Contracts.Items;

namespace PlatformTemplate.ServiceName.Application.Abstractions.Services;

public interface IServiceNameItemService
{
    Task<ServiceNameItemDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ServiceNameItemDto> CreateAsync(string name, CancellationToken cancellationToken = default);
}

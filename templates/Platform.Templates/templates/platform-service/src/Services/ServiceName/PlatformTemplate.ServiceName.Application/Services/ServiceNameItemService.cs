using PlatformTemplate.ServiceName.Application.Abstractions.Persistence;
using PlatformTemplate.ServiceName.Application.Abstractions.Services;
using PlatformTemplate.ServiceName.Application.Contracts.Items;
using PlatformTemplate.ServiceName.Domain.Models;
#if (UseOutbox)
using PlatformTemplate.ServiceName.Application.Abstractions.Messaging;
using PlatformTemplate.ServiceName.Application.Contracts.Events;
#endif
#if (redis)
using PlatformTemplate.BuildingBlocks.Caching;
#endif

namespace PlatformTemplate.ServiceName.Application.Services;

internal sealed class ServiceNameItemService : IServiceNameItemService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private readonly IServiceNameItemRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
#if (UseOutbox)
    private readonly IOutboxWriter _outboxWriter;
#endif
#if (redis)
    private readonly ICacheService _cache;
#endif

#if (UseOutbox && redis)
    public ServiceNameItemService(IServiceNameItemRepository repository, IUnitOfWork unitOfWork, IOutboxWriter outboxWriter, ICacheService cache)
#elseif (UseOutbox)
    public ServiceNameItemService(IServiceNameItemRepository repository, IUnitOfWork unitOfWork, IOutboxWriter outboxWriter)
#elseif (redis)
    public ServiceNameItemService(IServiceNameItemRepository repository, IUnitOfWork unitOfWork, ICacheService cache)
#else
    public ServiceNameItemService(IServiceNameItemRepository repository, IUnitOfWork unitOfWork)
#endif
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
#if (UseOutbox)
        _outboxWriter = outboxWriter;
#endif
#if (redis)
        _cache = cache;
#endif
    }

    public async Task<ServiceNameItemDto?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
#if (redis)
        var cacheKey = GetCacheKey(id);
        var cached = await _cache.GetAsync<ServiceNameItemDto>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }
#endif

        var item = await _repository.GetAsync(id, cancellationToken);
        if (item is null)
        {
            return null;
        }

        var result = Map(item);
#if (redis)
        await _cache.SetAsync(cacheKey, result, CacheDuration, cancellationToken);
#endif
        return result;
    }

    public async Task<ServiceNameItemDto> CreateAsync(string name, CancellationToken cancellationToken = default)
    {
        var item = ServiceNameItem.Create(name);
        _repository.Add(item);

#if (UseOutbox)
        var integrationEvent = new ServiceNameItemCreated(Guid.NewGuid(), item.Id, item.Name, item.CreatedAt);
        _outboxWriter.Write(ServiceNameTopics.ItemCreated, item.Id.ToString("N"), integrationEvent);
#endif

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var result = Map(item);
#if (redis)
        await _cache.SetAsync(GetCacheKey(item.Id), result, CacheDuration, cancellationToken);
#endif
        return result;
    }

    private static ServiceNameItemDto Map(ServiceNameItem item) =>
        new(item.Id, item.Name, item.CreatedAt);

    private static string GetCacheKey(Guid id) => "ServiceName:items:" + id.ToString("N");
}

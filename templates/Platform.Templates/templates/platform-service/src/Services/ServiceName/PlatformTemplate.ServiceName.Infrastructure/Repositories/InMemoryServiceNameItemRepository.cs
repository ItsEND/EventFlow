using PlatformTemplate.ServiceName.Application.Abstractions.Persistence;
using PlatformTemplate.ServiceName.Domain.Models;
using System.Collections.Concurrent;

namespace PlatformTemplate.ServiceName.Infrastructure.Repositories;

internal sealed class InMemoryServiceNameItemRepository : IServiceNameItemRepository
{
    private readonly ConcurrentDictionary<Guid, ServiceNameItem> _items = new();

    public Task<ServiceNameItem?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _items.TryGetValue(id, out var item);
        return Task.FromResult(item);
    }

    public void Add(ServiceNameItem item)
    {
        if (!_items.TryAdd(item.Id, item))
        {
            throw new InvalidOperationException($"Элемент {item.Id} уже существует.");
        }
    }
}

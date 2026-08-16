using EventFlow.Events.Application.Abstractions.Caching;

namespace EventApi.IntegrationTests.Infrastructure;

internal class RecordingCacheService : ICacheService
{
    public List<string> RemovedKeys { get; } = [];

    public Task<T?> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default) where T : class
    {
        return Task.FromResult<T?>(null);
    }

    public Task SetAsync<T>(string cacheKey, T value, TimeSpan expiration, CancellationToken cancellationToken = default) where T : class
    {
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        RemovedKeys.Add(cacheKey);
        return Task.CompletedTask;
    }


}

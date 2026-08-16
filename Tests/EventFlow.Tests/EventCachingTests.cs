using EventFlow.Events.Application.Abstractions.Caching;
using EventFlow.Events.Application.Abstractions.Repositories;
using EventFlow.Events.Application.Contracts;
using EventFlow.Events.Application.Contracts.Events;
using EventFlow.Events.Application.Services;
using EventFlow.Events.Domain.Models;
using Microsoft.Extensions.Options;
using NSubstitute;


namespace EventFlow.Tests;

public class EventCachingTests
{
    [Fact]
    public async Task GetEventAsync_WhenCacheHit_DoesNotCallRepository()
    {
        var repository = Substitute.For<IEventRepository>();
        var cache = Substitute.For<ICacheService>();

        var cached = new EventDto
        {
            Id = Guid.NewGuid(),
            Title = "Cached event",
            Description = null,
            TotalSeats = 100,
            AvailableSeats = 40,
            StartAt = DateTime.UtcNow.AddDays(1),
            EndAt = DateTime.UtcNow.AddDays(2)
        };
        var key = CacheKeys.Event(cached.Id);
        var ct = CancellationToken.None;

        cache.GetAsync<EventDto>(key, ct).Returns(Task.FromResult<EventDto?>(cached));

        var service = new EventService(repository, cache, Options.Create(new CacheOptions()));

        var result = await service.GetEventAsync(cached.Id, ct);

        Assert.Equal(cached, result);

        await repository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetEventAsync_WhenCacheMiss_ReadsRepositoryAndFillsCache()
    {
        var repository = Substitute.For<IEventRepository>();
        var cache = Substitute.For<ICacheService>();

        var entity = Event.Create(
            "Database event",
            null,
            50,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2));

        var ct = CancellationToken.None;
        var key = CacheKeys.Event(entity.Id);
        var ttl = TimeSpan.FromMinutes(10);

        cache.GetAsync<EventDto>(key, ct).Returns(Task.FromResult<EventDto?>(null));

        repository.GetByIdAsync(entity.Id, ct).Returns(Task.FromResult<Event?>(entity));

        var service = new EventService(repository, cache,
            Options.Create(new CacheOptions
            {
                EventTtl = ttl,
                TopEventsTtl = TimeSpan.FromMinutes(1)
            }));

        var result = await service.GetEventAsync(entity.Id, ct);

        Assert.Equal(entity.Id, result.Id);

        await repository.Received(1).GetByIdAsync(entity.Id, ct);

        await cache.Received(1).SetAsync(key,
            Arg.Is<EventDto>(dto => dto.Id == entity.Id), ttl, ct);
    }

    [Fact]
    public async Task UpdateEventAsync_AfterSaving_RemovesEventCache()
    {
        var repository = Substitute.For<IEventRepository>();
        var cache = Substitute.For<ICacheService>();

        var entity = Event.Create(
            "Old title",
            null,
            20,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2));

        var ct = CancellationToken.None;

        repository.GetByIdAsync(entity.Id, ct).Returns(Task.FromResult<Event?>(entity));

        var service = new EventService(repository, cache, Options.Create(new CacheOptions()));

        await service.UpdateEventAsync(entity.Id,
            new UpdateEventModel
            {
                Title = "New title",
                Description = "Updated",
                StartAt = entity.StartAt.AddHours(1),
                EndAt = entity.EndAt.AddHours(1)
            }, ct);

        await cache.Received(1).RemoveAsync(CacheKeys.Event(entity.Id), ct);

        Received.InOrder(() =>
        {
            _ = repository.SaveChangesAsync(ct);
            _ = cache.RemoveAsync(CacheKeys.Event(entity.Id), ct);
        });
    }
}

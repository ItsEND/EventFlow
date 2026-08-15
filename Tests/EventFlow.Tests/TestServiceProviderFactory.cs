using EventFlow.Bookings.Application.Abstractions.Messaging.Outbox;
using EventFlow.Bookings.Application.Abstractions.Repositories;
using EventFlow.Bookings.Application.Abstractions.Services;
using EventFlow.Bookings.Application.Services;
using EventFlow.Bookings.Infrastructure.Background;
using EventFlow.Bookings.Infrastructure.DataAccess;
using EventFlow.Bookings.Infrastructure.Messaging.Outbox;
using EventFlow.Bookings.Infrastructure.Repositories;
using EventFlow.Events.Application.Abstractions.Caching;
using EventFlow.Events.Application.Abstractions.Repositories;
using EventFlow.Events.Application.Abstractions.Services;
using EventFlow.Events.Infrastructure.DataAccess;
using EventFlow.Events.Infrastructure.Repositories;
using EventFlow.Users.Application.Abstractions.Repositories;
using EventFlow.Users.Application.Abstractions.Security;
using EventFlow.Users.Application.Abstractions.Services;
using EventFlow.Users.Application.Services;
using EventFlow.Users.Infrastructure.DataAccess;
using EventFlow.Users.Infrastructure.Repositories;
using EventFlow.Users.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EventFlow.Tests;

internal static class TestServiceProviderFactory
{
    public static ServiceProvider Create()
    {
        var testId = Guid.NewGuid().ToString("N");
        var services = new ServiceCollection();

        services.AddLogging();

        services.AddDbContext<EventDbContext>(options =>
            options.UseInMemoryDatabase($"events-{testId}"));
        services.AddDbContext<BookingDbContext>(options =>
            options.UseInMemoryDatabase($"bookings-{testId}"));
        services.AddDbContext<UsersDbContext>(options =>
            options.UseInMemoryDatabase($"users-{testId}"));

        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IOutboxWriter, OutboxWriter>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IEventService, Events.Application.Services.EventService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IUserService, UserService>();

        services.AddSingleton<IBookingTaskQueue, InMemoryBookingTaskQueue>();
        services.AddSingleton<ICacheService, NoOpCacheService>();
        services.AddSingleton(Options.Create(new CacheOptions()));
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton(
            Options.Create(new JwtOptions
            {
                Secret = "test-jwt-secret-key-long-enough-for-hmac-2026",
                Issuer = "EventFlow.Tests",
                Audience = "EventFlow.Tests.Client",
                LifetimeMinutes = 60
            }));
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        return services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateScopes = true,
                ValidateOnBuild = true
            });
    }
}

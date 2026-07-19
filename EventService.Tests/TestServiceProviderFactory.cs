using EventFlow.Application.Abstractions.Repositories;
using EventFlow.Application.Abstractions.Security;
using EventFlow.Application.Abstractions.Services;
using EventFlow.Infrastructure.Background;
using EventFlow.Infrastructure.DataAccess;
using EventFlow.Infrastructure.Repositories;
using EventFlow.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EventService.Tests;

/// <summary>
/// Создаёт отдельный DI-контейнер с уникальной InMemory-базой
/// для каждого теста.
/// </summary>
internal static class TestServiceProviderFactory
{
    public static ServiceProvider Create()
    {
        var dbName = Guid.NewGuid().ToString();

        var services = new ServiceCollection();

        services.AddLogging();

        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(dbName));

        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        services.AddScoped<IEventService, EventFlow.Application.Services.EventService>();
        services.AddScoped<IBookingService, EventFlow.Application.Services.BookingService>();
        services.AddScoped<IUserService, EventFlow.Application.Services.UserService>();
        services.AddSingleton<IBookingTaskQueue, InMemoryBookingTaskQueue>();
       
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

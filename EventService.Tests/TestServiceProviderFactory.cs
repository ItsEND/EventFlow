using EventFlow.Api.DataAccess;
using EventFlow.Api.Repositories;
using EventFlow.Api.Repositories.Interfaces;
using EventFlow.Api.Services;
using EventFlow.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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

        services.AddScoped<IEventService, EventFlow.Api.Services.EventService>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IBookingService, BookingService>();

        return services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateScopes = true,
                ValidateOnBuild = true
            });
    }
}
using EventFlow.Bookings.Application.Abstractions.Publisher;
using EventFlow.Bookings.Application.Abstractions.Repositories;
using EventFlow.Bookings.Application.Abstractions.Services;
using EventFlow.Bookings.Infrastructure.Background;
using EventFlow.Bookings.Infrastructure.DataAccess;
using EventFlow.Bookings.Infrastructure.Publisher;
using EventFlow.Bookings.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EventFlow.Bookings.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("BookingConnection")
            ?? throw new InvalidOperationException("Строка подключения 'BookingConnection' не найдена.");

        services.AddDbContext<BookingDbContext>(options =>
            options.UseNpgsql(connectionString));


        services.AddScoped<IBookingRepository, BookingRepository>();

        services.Configure<KafkaOptions>(configuration.GetSection(KafkaOptions.SectionName));

        services.AddSingleton<IBookingConfirmedPublisher, BookingConfirmedPublisher>();


        services.AddSingleton<IBookingTaskQueue, InMemoryBookingTaskQueue>();
        services.AddHostedService<BookingProcessingBackgroundService>();

        return services;
    }

    public static async Task ApplyMigrationsAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BookingDbContext>();

        await dbContext.Database.MigrateAsync();
    }
}

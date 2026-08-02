using EventFlow.Events.Application.Abstractions.Services;
using EventFlow.Events.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EventFlow.Events.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}

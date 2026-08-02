using EventFlow.Bookings.Application.Abstractions.Services;
using EventFlow.Bookings.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EventFlow.Bookings.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IBookingService, BookingService>();

        return services;
    }
}

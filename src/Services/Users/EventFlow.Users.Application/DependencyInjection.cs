using EventFlow.Users.Application.Abstractions.Services;
using EventFlow.Users.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EventFlow.Users.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {

        services.AddScoped<IUserService, UserService>();

        return services;
    }
}

using Microsoft.Extensions.DependencyInjection;
using PlatformTemplate.ServiceName.Application.Abstractions.Services;
using PlatformTemplate.ServiceName.Application.Services;

namespace PlatformTemplate.ServiceName.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IServiceNameItemService, ServiceNameItemService>();
        return services;
    }
}

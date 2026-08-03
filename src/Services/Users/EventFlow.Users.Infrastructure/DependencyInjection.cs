using EventFlow.Users.Application.Abstractions.Repositories;
using EventFlow.Users.Application.Abstractions.Security;
using EventFlow.Users.Infrastructure.DataAccess;
using EventFlow.Users.Infrastructure.Repositories;
using EventFlow.Users.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EventFlow.Users.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("UserConnection")
            ?? throw new InvalidOperationException("Строка подключения 'UserConnection' не найдена.");

        services.AddDbContext<UsersDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUserRepository, UserRepository>();


        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();


        return services;
    }

    public static async Task ApplyMigrationsAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        await dbContext.Database.MigrateAsync();
    }
}

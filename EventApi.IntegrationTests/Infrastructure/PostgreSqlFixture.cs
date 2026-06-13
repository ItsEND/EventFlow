using EventFlow.Api.DataAccess;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace EventApi.IntegrationTests.Infrastructure;

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private const string TestDatabase = "eventflow_tests";
    private const string AdminDatabase = "postgres";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").WithDatabase(TestDatabase).Build();

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    public AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres.GetConnectionString(),
                npgsqlOptions =>
                {
                    npgsqlOptions.UseAdminDatabase(AdminDatabase);
                })
            .Options;

        return new AppDbContext(options);
    }

    public async Task ResetDatabaseAsync(CancellationToken cancellationToken = default)
    {
        await using var context = CreateContext();

        await context.Database.EnsureDeletedAsync(cancellationToken);
        await context.Database.MigrateAsync(cancellationToken);
    }
}


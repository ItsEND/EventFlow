using EventFlow.Bookings.Infrastructure.DataAccess;
using EventFlow.Events.Infrastructure.DataAccess;
using EventFlow.Users.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;

namespace EventApi.IntegrationTests.Infrastructure;

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private const string AdminDatabase = "postgres";
    private const string EventsDatabase = "eventflow_events_tests";
    private const string BookingsDatabase = "eventflow_bookings_tests";
    private const string UsersDatabase = "eventflow_users_tests";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("eventflow_test_host")
        .Build();

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    public EventDbContext CreateEventsContext()
    {
        var options = new DbContextOptionsBuilder<EventDbContext>()
            .UseNpgsql(ConnectionString(EventsDatabase), options => options.UseAdminDatabase(AdminDatabase))
            .Options;

        return new EventDbContext(options);
    }

    public BookingDbContext CreateBookingsContext()
    {
        var options = new DbContextOptionsBuilder<BookingDbContext>()
            .UseNpgsql(ConnectionString(BookingsDatabase), options => options.UseAdminDatabase(AdminDatabase))
            .Options;

        return new BookingDbContext(options);
    }

    public UsersDbContext CreateUsersContext()
    {
        var options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseNpgsql(ConnectionString(UsersDatabase), options => options.UseAdminDatabase(AdminDatabase))
            .Options;

        return new UsersDbContext(options);
    }

    public async Task ResetDatabasesAsync(CancellationToken cancellationToken = default)
    {
        await ResetDatabaseAsync(CreateEventsContext, cancellationToken);
        await ResetDatabaseAsync(CreateBookingsContext, cancellationToken);
        await ResetDatabaseAsync(CreateUsersContext, cancellationToken);
    }

    private string ConnectionString(string database)
    {
        var builder = new NpgsqlConnectionStringBuilder(_postgres.GetConnectionString())
        {
            Database = database
        };

        return builder.ConnectionString;
    }

    private static async Task ResetDatabaseAsync<TContext>(Func<TContext> contextFactory, CancellationToken cancellationToken)
        where TContext : DbContext
    {
        await using var context = contextFactory();
        await context.Database.EnsureDeletedAsync(cancellationToken);
        await context.Database.MigrateAsync(cancellationToken);
    }
}

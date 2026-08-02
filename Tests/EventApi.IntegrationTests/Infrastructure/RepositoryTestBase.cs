using EventFlow.Bookings.Infrastructure.DataAccess;
using EventFlow.Events.Infrastructure.DataAccess;
using EventFlow.Users.Infrastructure.DataAccess;

namespace EventApi.IntegrationTests.Infrastructure;

public abstract class RepositoryTestBase : IAsyncLifetime
{
    protected RepositoryTestBase(PostgreSqlFixture fixture)
    {
        Fixture = fixture;
    }

    protected PostgreSqlFixture Fixture { get; }

    protected EventDbContext CreateEventsContext() => Fixture.CreateEventsContext();

    protected BookingDbContext CreateBookingsContext() => Fixture.CreateBookingsContext();

    protected UsersDbContext CreateUsersContext() => Fixture.CreateUsersContext();

    public async ValueTask InitializeAsync()
    {
        await Fixture.ResetDatabasesAsync();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

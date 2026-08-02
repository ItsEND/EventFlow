using EventFlow.Events.Infrastructure.DataAccess;

namespace EventApi.IntegrationTests.Infrastructure;

public abstract class RepositoryTestBase : IAsyncLifetime
{
    protected RepositoryTestBase(PostgreSqlFixture fixture)
    {
        Fixture = fixture;
    }

    protected PostgreSqlFixture Fixture { get; }

    protected EventDbContext CreateContext()
    {
        return Fixture.CreateContext();
    }

    public async ValueTask InitializeAsync()
    {
        await Fixture.ResetDatabaseAsync();
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}

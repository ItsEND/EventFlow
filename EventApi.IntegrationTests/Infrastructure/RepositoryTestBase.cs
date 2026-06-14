using EventFlow.Api.DataAccess;

namespace EventApi.IntegrationTests.Infrastructure;

public abstract class RepositoryTestBase : IAsyncLifetime
{
    protected RepositoryTestBase(PostgreSqlFixture fixture)
    {
        Fixture = fixture;
    }

    protected PostgreSqlFixture Fixture { get; }

    protected AppDbContext CreateContext()
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
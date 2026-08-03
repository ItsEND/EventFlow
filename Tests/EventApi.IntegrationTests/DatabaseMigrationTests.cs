using EventApi.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EventApi.IntegrationTests;

[Collection(TestCollections.PostgreSql)]
public class DatabaseMigrationTests(PostgreSqlFixture fixture) : RepositoryTestBase(fixture)
{
    [Fact]
    public async Task Migrations_ShouldCreateOnlyServiceOwnedTables()
    {
        await using var eventsContext = CreateEventsContext();
        await using var bookingsContext = CreateBookingsContext();
        await using var usersContext = CreateUsersContext();

        Assert.True(await TableExistsAsync(eventsContext, "event"));
        Assert.False(await TableExistsAsync(eventsContext, "booking"));
        Assert.False(await TableExistsAsync(eventsContext, "users"));

        Assert.True(await TableExistsAsync(bookingsContext, "booking"));
        Assert.False(await TableExistsAsync(bookingsContext, "event"));
        Assert.False(await TableExistsAsync(bookingsContext, "users"));

        Assert.True(await TableExistsAsync(usersContext, "users"));
        Assert.False(await TableExistsAsync(usersContext, "event"));
        Assert.False(await TableExistsAsync(usersContext, "booking"));
    }

    private static async Task<bool> TableExistsAsync(DbContext context, string tableName)
    {
        await context.Database.OpenConnectionAsync(CancellationToken.None);

        try
        {
            await using var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = """
                SELECT EXISTS (SELECT 1
                    FROM information_schema.tables
                    WHERE table_schema = 'public'
                      AND table_name = @tableName
                );
                """;
            var parameter = command.CreateParameter();
            parameter.ParameterName = "tableName";
            parameter.Value = tableName;
            command.Parameters.Add(parameter);

            var result = await command.ExecuteScalarAsync(CancellationToken.None);
            return result is bool exists && exists;
        }
        finally
        {
            await context.Database.CloseConnectionAsync();
        }
    }
}

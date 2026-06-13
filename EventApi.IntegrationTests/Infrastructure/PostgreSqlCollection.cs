namespace EventApi.IntegrationTests.Infrastructure;

[CollectionDefinition(TestCollections.PostgreSql, DisableParallelization = true)]
public sealed class PostgreSqlCollection : ICollectionFixture<PostgreSqlFixture>
{
}
public static class TestCollections
{
    public const string PostgreSql = "PostgreSQL repository tests";
}
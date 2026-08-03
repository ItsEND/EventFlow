using EventApi.IntegrationTests.Infrastructure;
using EventFlow.Users.Domain.Models;
using EventFlow.Users.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EventApi.IntegrationTests;

[Collection(TestCollections.PostgreSql)]
public sealed class UserRepositoryTests(PostgreSqlFixture fixture) : RepositoryTestBase(fixture)
{
    [Fact]
    public async Task Add_AndSaveChangesAsync_ShouldPersistUser()
    {
        var user = User.Create("integration-user", "PASSWORD_HASH", UserRole.Admin);

        await using (var context = CreateUsersContext())
        {
            var repository = new UserRepository(context);
            repository.Add(user);
            await repository.SaveChangesAsync(CancellationToken.None);
        }

        await using var verificationContext = CreateUsersContext();
        var saved = await verificationContext.Users.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == user.Id, CancellationToken.None);

        Assert.NotNull(saved);
        Assert.Equal("integration-user", saved.Login);
        Assert.Equal(UserRole.Admin, saved.Role);
    }

    [Fact]
    public async Task GetByLoginAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        await using var context = CreateUsersContext();
        var repository = new UserRepository(context);

        var user = await repository.GetByLoginAsync("missing-user", CancellationToken.None);

        Assert.Null(user);
    }
}

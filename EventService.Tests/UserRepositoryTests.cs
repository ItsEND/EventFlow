using EventFlow.Events.Application.Abstractions.Repositories;
using EventFlow.Events.Domain.Models;
using Microsoft.Extensions.DependencyInjection;

namespace EventService.Tests;

public class UserRepositoryTests : IDisposable
{
    private readonly ServiceProvider _provider;

    public UserRepositoryTests()
    {
        _provider = TestServiceProviderFactory.Create();
    }

    public void Dispose()
    {
        _provider.Dispose();
    }

    [Fact]
    public async Task Add_ShouldPersistUser()
    {
        var ct = TestContext.Current.CancellationToken;
        var user = User.Create("new-user", "PASSWORD_HASH");

        await using (var scope = _provider.CreateAsyncScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            repository.Add(user);
            await repository.SaveChangesAsync(ct);
        }

        await using (var scope = _provider.CreateAsyncScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            var savedUser = await repository.GetByLoginAsync("new-user",ct);

            Assert.NotNull(savedUser);
            Assert.Equal(user.Id, savedUser.Id);
            Assert.Equal("new-user", savedUser.Login);
            Assert.Equal("PASSWORD_HASH", savedUser.PasswordHash);
            Assert.Equal(UserRole.User, savedUser.Role);
        }
    }

    [Fact]
    public async Task GetByLoginAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var scope = _provider.CreateAsyncScope();

        var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        var user = await repository.GetByLoginAsync("missing-user", ct);

        Assert.Null(user);
    }
}


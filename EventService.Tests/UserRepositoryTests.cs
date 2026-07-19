using EventFlow.Application.Abstractions.Repositories;
using EventFlow.Domain.Models;
using Microsoft.Extensions.DependencyInjection;

namespace EventService.Tests;

public class UserRepositoryTests
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
        var user = User.Create("new-user", "PASSWORD_HASH");

        await using (var scope = _provider.CreateAsyncScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            repository.Add(user);
            await repository.SaveChangesAsync();
        }

        await using (var scope = _provider.CreateAsyncScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            var savedUser = await repository.GetByLoginAsync("new-user");

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
        await using var scope = _provider.CreateAsyncScope();

        var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        var user = await repository.GetByLoginAsync("missing-user");

        Assert.Null(user);
    }
}


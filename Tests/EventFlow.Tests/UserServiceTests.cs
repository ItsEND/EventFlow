using EventFlow.Users.Application.Abstractions.Repositories;
using EventFlow.Users.Application.Abstractions.Security;
using EventFlow.Users.Application.Abstractions.Services;
using EventFlow.Users.Application.Contracts;
using EventFlow.Users.Application.Exceptions;
using EventFlow.Users.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace EventFlow.Tests;

public class UserServiceTests : IDisposable
{
    private readonly ServiceProvider _provider = TestServiceProviderFactory.Create();

    public void Dispose() => _provider.Dispose();

    [Fact]
    public async Task RegisterAsync_ShouldSaveUserWithHashedPassword()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var scope = _provider.CreateAsyncScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await userService.RegisterAsync(Registration("alex", "secret123"), ct);
        var user = await userRepository.GetByLoginAsync("alex", ct);

        Assert.NotNull(user);
        Assert.NotEqual("secret123", user.PasswordHash);
        Assert.True(passwordHasher.Verify("secret123", user.PasswordHash));
        Assert.Equal(UserRole.User, user.Role);
    }

    [Fact]
    public async Task RegisterAsync_ShouldSaveAdminRole()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var scope = _provider.CreateAsyncScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        await userService.RegisterAsync(Registration("admin", "admin-password", "Admin"), ct);
        var user = await userRepository.GetByLoginAsync("admin", ct);

        Assert.NotNull(user);
        Assert.Equal(UserRole.Admin, user.Role);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrow_WhenLoginAlreadyExists()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var scope = _provider.CreateAsyncScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        await userService.RegisterAsync(Registration("alex", "first-password"), ct);

        await Assert.ThrowsAsync<ValidationException>(() => userService.RegisterAsync(Registration("alex", "second-password"), ct));
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreCorrect()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var scope = _provider.CreateAsyncScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        await userService.RegisterAsync(Registration("alex", "secret123"), ct);

        var token = await userService.LoginAsync("alex", "secret123", ct);

        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.Equal(3, token.Split('.').Length);
    }

    [Theory]
    [InlineData("alex", "incorrect-password")]
    [InlineData("missing-user", "some-password")]
    public async Task LoginAsync_ShouldThrowInvalidCredentials_WhenCredentialsAreInvalid(string login, string password)
    {
        var ct = TestContext.Current.CancellationToken;
        await using var scope = _provider.CreateAsyncScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        await userService.RegisterAsync(Registration("alex", "correct-password"), ct);

        var exception = await Assert.ThrowsAsync<UserException>(() => userService.LoginAsync(login, password, ct));

        Assert.Equal(UserErrorCode.InvalidCredentials, exception.Code);
        Assert.Equal("Неверный логин или пароль.", exception.Message);
    }

    private static RegisterUserModel Registration(string login, string password, string? role = null) =>
        new()
        {
            Login = login,
            Password = password,
            Role = role
        };
}

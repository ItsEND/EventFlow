using EventFlow.Application.Abstractions.Repositories;
using EventFlow.Application.Abstractions.Security;
using EventFlow.Application.Abstractions.Services;
using EventFlow.Application.Exceptions;
using EventFlow.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace EventService.Tests;

public class UserServiceTests : IDisposable
{
    private readonly ServiceProvider _provider;

    public UserServiceTests()
    {
        _provider = TestServiceProviderFactory.Create();
    }

    public void Dispose()
    {
        _provider.Dispose();
    }

    [Fact]
    public async Task RegisterAsync_ShouldSaveUserWithHashedPassword()
    {
        var ct = TestContext.Current.CancellationToken;

        await using var scope = _provider.CreateAsyncScope();

        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await userService.RegisterAsync("alex", "secret123", ct: ct);

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

        await userService.RegisterAsync("admin", "admin-password", UserRole.Admin, ct);

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

        await userService.RegisterAsync("alex", "first-password", ct: ct);

        var action = () => userService.RegisterAsync("alex", "second-password", ct: ct);

        await Assert.ThrowsAsync<ValidationException>(action);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreCorrect()
    {
        var ct = TestContext.Current.CancellationToken;

        await using var scope = _provider.CreateAsyncScope();

        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        await userService.RegisterAsync("alex", "secret123", ct: ct);

        var token = await userService.LoginAsync("alex", "secret123", ct);

        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.Equal(3, token.Split('.').Length);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowNotFound_WhenPasswordIsIncorrect()
    {
        var ct = TestContext.Current.CancellationToken;

        await using var scope = _provider.CreateAsyncScope();

        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        await userService.RegisterAsync("alex", "correct-password", ct: ct);

        var action = () => userService.LoginAsync("alex", "incorrect-password", ct);

        var exception = await Assert.ThrowsAsync<AppException>(action);

        Assert.Equal(AppErrorCode.NotFound, exception.Code);

        Assert.Equal("Неверный логин или пароль.", exception.Message);
    }
    [Fact]
    public async Task LoginAsync_ShouldThrowNotFound_WhenLoginDoesNotExist()
    {
        var ct = TestContext.Current.CancellationToken;

        await using var scope = _provider.CreateAsyncScope();

        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        var action = () => userService.LoginAsync("missing-user", "some-password", ct);

        var exception = await Assert.ThrowsAsync<AppException>(action);

        Assert.Equal(AppErrorCode.NotFound, exception.Code);

        Assert.Equal("Неверный логин или пароль.", exception.Message);
    }
}

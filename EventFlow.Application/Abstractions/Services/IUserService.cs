using EventFlow.Domain.Models;

namespace EventFlow.Application.Abstractions.Services;

public interface IUserService
{
    Task RegisterAsync(string login, string password, UserRole role = UserRole.User, CancellationToken ct = default);

    Task<string> LoginAsync(string login, string password, CancellationToken ct = default);

}

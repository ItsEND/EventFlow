using EventFlow.Application.Dtos.Users;

namespace EventFlow.Application.Abstractions.Services;

public interface IUserService
{
    Task RegisterAsync(RegisterUserModel model, CancellationToken ct = default);

    Task<string> LoginAsync(string login, string password, CancellationToken ct = default);

}

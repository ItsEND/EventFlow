using EventFlow.Users.Domain.Models;

namespace EventFlow.Users.Application.Abstractions.Repositories;

public interface IUserRepository
{
    void Add(User user);

    Task<User?> GetByLoginAsync(string login, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}

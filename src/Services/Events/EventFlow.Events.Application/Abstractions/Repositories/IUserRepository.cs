using EventFlow.Events.Domain.Models;

namespace EventFlow.Events.Application.Abstractions.Repositories;

public interface IUserRepository
{
    void Add(User user);

    Task<User?> GetByLoginAsync(string login, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}

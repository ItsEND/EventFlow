using EventFlow.Users.Application.Abstractions.Repositories;
using EventFlow.Users.Domain.Models;
using EventFlow.Users.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Users.Infrastructure.Repositories;

public class UserRepository(UsersDbContext db) : IUserRepository
{
    public void Add(User user)
    {
        db.Users.Add(user);
    }

    public Task<User?> GetByLoginAsync(string login, CancellationToken ct = default)
    {
        return db.Users.FirstOrDefaultAsync(user => user.Login == login.Trim(), ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        return db.SaveChangesAsync(ct);
    }
}

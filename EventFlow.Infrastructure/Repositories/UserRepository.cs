using EventFlow.Application.Abstractions.Repositories;
using EventFlow.Domain.Models;
using EventFlow.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Infrastructure.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
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

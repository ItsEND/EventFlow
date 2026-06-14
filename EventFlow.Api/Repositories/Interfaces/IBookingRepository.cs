using EventFlow.Api.Models;

namespace EventFlow.Api.Repositories.Interfaces;

public interface IBookingRepository
{
    void Add(Booking booking);
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Guid>> GetPendingIdsAsync(CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

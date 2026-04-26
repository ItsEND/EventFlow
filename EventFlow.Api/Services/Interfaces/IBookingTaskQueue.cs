using EventFlow.Api.Models;

namespace EventFlow.Api.Services.Interfaces;

public interface IBookingTaskQueue
{
    ValueTask EnqueueAsync(Guid bookingId, CancellationToken ct);
    IAsyncEnumerable<Guid> DequeueAllAsync(CancellationToken ct);
}

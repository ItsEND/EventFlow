namespace EventFlow.Application.Abstractions.Services;

/// <summary>
/// Очередь задач для фоновой обработки бронирований.
/// </summary>
public interface IBookingTaskQueue
{
    ValueTask EnqueueAsync(Guid bookingId, CancellationToken ct);
    IAsyncEnumerable<Guid> DequeueAllAsync(CancellationToken ct);
}

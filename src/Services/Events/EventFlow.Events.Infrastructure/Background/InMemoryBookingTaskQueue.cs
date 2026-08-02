using EventFlow.Events.Application.Abstractions.Services;
using System.Threading.Channels;

namespace EventFlow.Events.Infrastructure.Background;

/// <summary>
/// Внутрипроцессная очередь задач для фоновой обработки бронирований.
/// </summary>
public class InMemoryBookingTaskQueue : IBookingTaskQueue
{
    private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>(
        new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

    public async ValueTask EnqueueAsync(Guid bookingId, CancellationToken ct)
    {
        await _channel.Writer.WriteAsync(bookingId, ct);
    }

    public async IAsyncEnumerable<Guid> DequeueAllAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        while (await _channel.Reader.WaitToReadAsync(ct))
        {
            while (_channel.Reader.TryRead(out var bookingId))
            {
                yield return bookingId;
            }
        }
    }
}

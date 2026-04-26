using EventFlow.Api.Services.Interfaces;
using EventFlow.Api.Models;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace EventFlow.Api.Services;

public class InMemoryBookingTaskQueue : IBookingTaskQueue
{
    private readonly Channel<Guid> _channel;

    public InMemoryBookingTaskQueue()
    {
        var options = new BoundedChannelOptions(capacity: 100)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        };
        _channel = Channel.CreateBounded<Guid>(options);
    }

    public ValueTask EnqueueAsync(Guid bookingId, CancellationToken ct)
    {
        return _channel.Writer.WriteAsync(bookingId, ct);
    }

    public IAsyncEnumerable<Guid> DequeueAllAsync(CancellationToken ct)
    {
        return _channel.Reader.ReadAllAsync(ct);
    }
}



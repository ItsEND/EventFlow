using EventFlow.Api.Models;
using EventFlow.Api.Services.Interfaces;
using System.Collections.Concurrent;

namespace EventFlow.Api.Services;

public class InMemoryBookingTaskQueue : IBookingTaskQueue
{
    private readonly ConcurrentQueue<Booking> _queue = new();
    public void Enqueue(Booking bookingTask)
    {
        _queue.Enqueue(bookingTask);
    }

    public bool TryDeQueue(out Booking bookingTask)
    {
        return _queue.TryDequeue(out bookingTask);
    }
}



using EventFlow.Api.Models;

namespace EventFlow.Api.Services.Interfaces;

public interface IBookingTaskQueue
{
    void Enqueue(Booking bookingTask);
    bool TryDeQueue(out Booking bookingTask);
}

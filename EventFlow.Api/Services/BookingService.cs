using EventFlow.Api.Models;
using EventFlow.Api.Services.Interfaces;
using System.Collections.Concurrent;

namespace EventFlow.Api.Services;

public class BookingService : IBookingService
{
    private readonly IEventService _eventService;
    private readonly IBookingTaskQueue _taskQueue;
    private readonly ConcurrentDictionary<Guid, Booking> _bookings;

    public BookingService(IEventService eventService, IBookingTaskQueue taskQueue, IEnumerable<Booking>? bookings = null)
    {
        _eventService = eventService;

        var initDict = bookings?.ToDictionary(b => b.Id, b => b) ?? [];

        _taskQueue = taskQueue;
        _bookings = new ConcurrentDictionary<Guid, Booking>(initDict);
    }

    public async Task<Booking> CreateBookingAsync(Guid eventId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        _eventService.GetEvent(eventId);


        var booking = Booking.Create(eventId);
        _bookings.TryAdd(booking.Id, booking);

        await _taskQueue.EnqueueAsync(booking.Id, ct);
        return booking;
    }

    public Task<Booking> GetBookingByIdAsync(Guid bookingId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var res = _bookings.TryGetValue(bookingId, out var booking);

        return !res ? throw new NotFoundException("Booking", bookingId) : Task.FromResult(booking);
    }

    public async Task<Booking> ProcessBookingAsync(Guid bookingId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var booking = await GetBookingByIdAsync(bookingId, ct);

        booking.Confirm();

        return booking;
    }
}

using EventFlow.Api.Models;
using EventFlow.Api.Services.Interfaces;
using System.Collections.Concurrent;

namespace EventFlow.Api.Services;

public class BookingService : IBookingService
{
    private readonly IEventService _eventService;

    private readonly ConcurrentDictionary<Guid, Booking> _bookings;

    public BookingService(IEventService eventService, IEnumerable<Booking>? bookings = null)
    {
        _eventService = eventService;
        var initDict = bookings?.ToDictionary(b => b.Id, b => b) ?? new Dictionary<Guid, Booking>();

        _bookings = new ConcurrentDictionary<Guid, Booking>(initDict);
    }

    public Task<Booking> CreateBookingAsync(Guid eventId)
    {
        var eventEntity = _eventService.GetEvent(eventId);

        var booking = Booking.Create(eventId);

        _bookings.TryAdd(booking.Id, booking);

        return Task.FromResult(booking);
    }

    public Task<Booking> GetBookingByIdAsync(Guid bookingId)
    {
        var res = _bookings.TryGetValue(bookingId, out var booking);

        if (!res)
        {
            throw new NotFoundException("Booking", bookingId);
        }

        return Task.FromResult(booking!);
    }
}

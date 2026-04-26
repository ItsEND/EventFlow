using EventFlow.Api.Models;
namespace EventFlow.Api.Services.Interfaces;

public interface IBookingService
{
    Task<Booking> CreateBookingAsync(Guid eventId, CancellationToken ct);
    Task<Booking> GetBookingByIdAsync(Guid bookingId, CancellationToken ct);
    Task<Booking> ProcessBookingAsync(Guid bookingId, CancellationToken ct);
}

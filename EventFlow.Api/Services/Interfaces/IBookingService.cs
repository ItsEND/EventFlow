using EventFlow.Api.Models;
namespace EventFlow.Api.Services.Interfaces;

public interface IBookingService
{
    Task<Booking> CreateBookingAsync(Guid eventId);
    Task<Booking> GetBookingByIdAsync(Guid bookingId);
}

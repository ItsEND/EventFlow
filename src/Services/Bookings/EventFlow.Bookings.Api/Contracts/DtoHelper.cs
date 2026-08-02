using EventFlow.Bookings.Api.Contracts.Booking;
using EventFlow.Bookings.Application.Contracts;

namespace EventFlow.Bookings.Api.Contracts;

/// <summary>
/// Преобразует DTO слоя Application в HTTP DTO.
/// </summary>
public static class DtoHelper
{
  

    /// <summary>
    /// Преобразует внутреннюю модель бронирования в DTO ответа.
    /// </summary>
    /// <param name="booking">DTO брони из слоя Application.</param>
    /// <returns>Объект ответа с данными брони.</returns>
    public static BookingResponse ToBookingResponse(BookingDto booking) => new()
    {
        Id = booking.Id,
        EventId = booking.EventId,
        UserId = booking.UserId,
        Status = booking.Status,
        CreatedAt = booking.CreatedAt,
        ProcessedAt = booking.ProcessedAt
    };
}

using EventFlow.Api.Contracts.Booking;
using EventFlow.Api.Contracts.Events;
using EventFlow.Application.Dtos.Booking;
using EventFlow.Application.Dtos.Events;

namespace EventFlow.Api.Contracts;

/// <summary>
/// Преобразует DTO слоя Application в HTTP DTO.
/// </summary>
public static class DtoHelper
{
    /// <summary>
    /// Преобразует внутреннюю модель мероприятия в DTO ответа.
    /// </summary>
    /// <param name="ev">DTO мероприятия из слоя Application.</param>
    /// <returns>Объект ответа с данными мероприятия.</returns>
    public static EventResponse ToEventResponse(EventDto ev) => new()
    {
        Id = ev.Id,
        Title = ev.Title,
        Description = ev.Description,
        TotalSeats = ev.TotalSeats,
        AvailableSeats = ev.AvailableSeats,
        StartAt = ev.StartAt,
        EndAt = ev.EndAt
    };

    /// <summary>
    /// Преобразует внутреннюю модель бронирования в DTO ответа.
    /// </summary>
    /// <param name="booking">DTO брони из слоя Application.</param>
    /// <returns>Объект ответа с данными брони.</returns>
    public static BookingResponse ToBookingResponse(BookingDto booking) => new()
    {
        Id = booking.Id,
        EventId = booking.EventId,
        Status = booking.Status,
        CreatedAt = booking.CreatedAt,
        ProcessedAt = booking.ProcessedAt
    };
}

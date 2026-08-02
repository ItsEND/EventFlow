using EventFlow.Events.Api.Contracts.Booking;
using EventFlow.Events.Api.Contracts.Events;
using EventFlow.Events.Application.Dtos.Booking;
using EventFlow.Events.Application.Dtos.Events;

namespace EventFlow.Events.Api.Contracts;

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
        UserId = booking.UserId,
        Status = booking.Status,
        CreatedAt = booking.CreatedAt,
        ProcessedAt = booking.ProcessedAt
    };
}

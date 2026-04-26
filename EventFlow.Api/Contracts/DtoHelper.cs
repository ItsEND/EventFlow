using EventFlow.Api.Contracts.Booking;
using EventFlow.Api.Contracts.Events;
using EventFlow.Api.Models;

namespace EventFlow.Api.Contracts
{
    public static class DtoHelper
    {
        /// <summary>
        /// Преобразует внутреннюю модель мероприятия в DTO ответа.
        /// </summary>
        /// <param name="ev">Мероприятие доменной модели.</param>
        /// <returns>Объект ответа с данными мероприятия.</returns>
        public static EventResponse ToEventResponse(Event ev) => new()
        {
            Id = ev.Id,
            Title = ev.Title,
            Description = ev.Description,
            StartAt = ev.StartAt,
            EndAt = ev.EndAt
        };
        /// <summary>
        /// Преобразует внутреннюю модель бронирования в DTO ответа.
        /// </summary>
        /// <param name="booking">Мероприятие доменной модели.</param>
        /// <returns>Объект ответа с данными бронирования</returns>
        public static BookingResponse ToBookingResponse(Models.Booking booking) => new()
        {
            Id = booking.Id,
            EventId = booking.EventId,
            Status = booking.Status.ToString(),
        };
    }
}

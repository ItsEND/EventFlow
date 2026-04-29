using EventFlow.Api.Models;

namespace EventFlow.Api.Services.Interfaces;

/// <summary>
/// Определяет операции для создания, получения и фоновой обработки бронирований.
/// </summary>
public interface IBookingService
{
    /// <summary>
    /// Создаёт бронь для указанного мероприятия.
    /// Бронь создаётся в статусе Pending и передаётся в очередь фоновой обработки.
    /// </summary>
    /// <param name="eventId">Идентификатор мероприятия.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Созданная бронь.</returns>
    Task<Booking> CreateBookingAsync(Guid eventId, CancellationToken ct);

    /// <summary>
    /// Возвращает бронь по её идентификатору.
    /// </summary>
    /// <param name="bookingId">Идентификатор брони.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Найденная бронь.</returns>
    Task<Booking> GetBookingByIdAsync(Guid bookingId, CancellationToken ct);

    /// <summary>
    /// Выполняет обработку брони.
    /// В текущей реализации переводит бронь из Pending в Confirmed.
    /// </summary>
    /// <param name="bookingId">Идентификатор брони.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Обработанная бронь.</returns>
    Task<Booking> ProcessBookingAsync(Guid bookingId, CancellationToken ct);
}
using EventFlow.Events.Application.Dtos.Booking;

namespace EventFlow.Events.Application.Abstractions.Services;

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
    Task<BookingDto> CreateBookingAsync(Guid eventId, Guid userId, CancellationToken ct);

    /// <summary>
    /// Возвращает бронь по её идентификатору.
    /// </summary>
    /// <param name="bookingId">Идентификатор брони.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Найденная бронь.</returns>
    Task<BookingDto> GetBookingByIdAsync(Guid bookingId, CancellationToken ct);

    /// <summary>
    /// Выполняет обработку брони.
    /// В текущей реализации переводит бронь из Pending в Confirmed.
    /// </summary>
    /// <param name="bookingId">Идентификатор брони.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Обработанная бронь.</returns>
    Task<BookingDto> ProcessBookingAsync(Guid bookingId, CancellationToken ct);

    /// <summary>
    /// Отменяет бронь с проверкой прав текущего пользователя.
    /// </summary>
    Task CancelBookingAsync(Guid bookingId, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken);
}

namespace EventFlow.Bookings.Application.Exceptions;

/// <summary>
/// Коды ошибок, которые слой Application отдает внешним слоям.
/// </summary>
public enum AppErrorCode
{
    NotFound,
    NoAvailableSeats,
    EventAlreadyStarted,
    BookingLimitExceeded,
    Forbidden
}

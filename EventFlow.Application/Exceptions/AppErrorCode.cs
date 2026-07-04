namespace EventFlow.Application.Exceptions;

/// <summary>
/// Коды ошибок, которые слой Application отдает внешним слоям.
/// </summary>
public enum AppErrorCode
{
    NotFound,
    NoAvailableSeats
}

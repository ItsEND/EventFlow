namespace EventFlow.Api.Models;

/// <summary>
/// Статус брони.
/// </summary>
public enum BookingStatus
{
    /// <summary>
    /// Бронь создана и ожидает фоновой обработки.
    /// </summary>
    Pending,

    /// <summary>
    /// Бронь подтверждена.
    /// </summary>
    Confirmed,

    /// <summary>
    /// Бронь отклонена.
    /// </summary>
    Rejected
}
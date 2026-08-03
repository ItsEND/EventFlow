namespace EventFlow.Contracts;

public sealed record BookingConfirmed(
    Guid BookingId,
    Guid EventId,
    Guid UserId,
    int SeatCount,
    DateTime ConfirmedAt
    );

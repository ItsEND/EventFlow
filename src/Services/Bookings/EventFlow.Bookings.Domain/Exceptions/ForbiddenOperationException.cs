namespace EventFlow.Bookings.Domain.Exceptions;

/// <summary>
/// Операция запрещена для текущего пользователя.
/// </summary>
public class ForbiddenOperationException : Exception
{
    public ForbiddenOperationException(string message)
        : base(message)
    {
    }
}

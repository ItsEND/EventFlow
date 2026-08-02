namespace EventFlow.Events.Domain.Exceptions;

public class BookingLimitExceededException : Exception
{
    public BookingLimitExceededException(string message)
       : base(message)
    {
    }
}

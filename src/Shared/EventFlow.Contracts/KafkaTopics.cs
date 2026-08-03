namespace EventFlow.Contracts;

public static class KafkaTopics
{
    public const string BookingConfirmed = "booking-confirmed";
    public const string BookingConfirmedDeadLetter = "booking-confirmed-dlq";
}

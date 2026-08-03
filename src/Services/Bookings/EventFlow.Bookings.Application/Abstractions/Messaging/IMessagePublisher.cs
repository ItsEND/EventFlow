namespace EventFlow.Bookings.Application.Abstractions.Messaging;

public interface IMessagePublisher
{
    Task PublishAsync(string topic, string messageKey, string payload, CancellationToken cancellationToken);
}
using Confluent.Kafka;
using EventFlow.Bookings.Application.Abstractions.Publisher;
using EventFlow.Contracts;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace EventFlow.Bookings.Infrastructure.Publisher;

public class BookingConfirmedPublisher : IBookingConfirmedPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;

    public BookingConfirmedPublisher(IOptions<KafkaOptions> options)
    {
        var bootstrapServers = options.Value.BootstrapServers;

        if (string.IsNullOrWhiteSpace(bootstrapServers))
        {
            throw new InvalidOperationException("Адрес брокера Kafka (BootstrapServers) не настроен.");
        }

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            Acks = Acks.All,
        };

        _producer = new ProducerBuilder<string, string>(producerConfig).Build();
    }



    public async Task PublishAsync(BookingConfirmed message, CancellationToken cancellationToken)
    {
        var kafaMessage = new Message<string, string>
        {
            Key = message.EventId.ToString(),
            Value = JsonSerializer.Serialize(message)
        };

        await _producer.ProduceAsync(KafkaTopics.BookingConfirmed, kafaMessage, cancellationToken);
    }
    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }

}

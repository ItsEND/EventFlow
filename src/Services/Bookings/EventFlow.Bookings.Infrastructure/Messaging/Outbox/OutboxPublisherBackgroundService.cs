using EventFlow.Bookings.Application.Abstractions.Messaging;
using EventFlow.Bookings.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventFlow.Bookings.Infrastructure.Messaging.Outbox;

public sealed class OutboxPublisherBackgroundService(IServiceScopeFactory scopeFactory, ILogger<OutboxPublisherBackgroundService> logger)
    : BackgroundService
{
    private const int BatchSize = 50;

    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Фоновый обработчик Outbox запущен.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Ошибка фоновой обработки Outbox.");
            }

            try
            {
                await Task.Delay(PollingInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        logger.LogInformation("Фоновый обработчик Outbox остановлен.");
    }

    private async Task PublishBatchAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();

        var messages = await dbContext.OutboxMessages
          .Where(message => message.PublishedAt == null)
          .OrderBy(message => message.OccurredAt)
          .Take(BatchSize)
          .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                await publisher.PublishAsync(message.Topic, message.MessageKey, message.Payload, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                message.RegisterFailure(exception.Message);

                await dbContext.SaveChangesAsync(cancellationToken);

                logger.LogError(
                    exception,
                    "Не удалось опубликовать Outbox-сообщение {MessageId}. " +
                    "Попытка: {Attempt}",
                    message.Id,
                    message.Attempts);

                continue;
            }

            message.MarkPublished(DateTime.UtcNow);

            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Outbox-сообщение {MessageId} опубликовано в топик {Topic}.",
                message.Id,
                message.Topic);
        }

    }
}
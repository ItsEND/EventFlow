using EventFlow.Events.Application.Abstractions.Services;
using EventFlow.Events.Application.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventFlow.Events.Infrastructure.Background;

/// <summary>
/// Фоновый сервис обработки бронирований.
/// Получает идентификаторы броней из очереди и передает их в сервис Application.
/// </summary>
public class BookingProcessingBackgroundService(
    IServiceScopeFactory scopeFactory,
    IBookingTaskQueue bookingTaskQueue,
    ILogger<BookingProcessingBackgroundService> logger) : BackgroundService
{
    /// <summary>
    /// Выполняет непрерывную фоновую обработку броней до остановки приложения.
    /// </summary>
    /// <param name="stoppingToken">Токен остановки фонового сервиса.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var bookingId in bookingTaskQueue.DequeueAllAsync(stoppingToken))
            {
                await ProcessBookingAsync(bookingId, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Фоновая обработка бронирований остановлена.");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Фоновый сервис обработки бронирований завершился с ошибкой.");
            throw;
        }
    }

    private async Task ProcessBookingAsync(Guid bookingId, CancellationToken stoppingToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

            await bookingService.ProcessBookingAsync(bookingId, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            throw;
        }
        catch (AppException ex) when (ex.Code == AppErrorCode.NotFound)
        {
            logger.LogWarning(ex, "Бронирование {BookingId} не найдено во время фоновой обработки.", bookingId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при обработке брони {BookingId}", bookingId);
        }
    }
}

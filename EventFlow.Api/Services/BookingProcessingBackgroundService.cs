using EventFlow.Api.Repositories.Interfaces;
using EventFlow.Api.Services.Interfaces;

namespace EventFlow.Api.Services;

/// <summary>
/// Фоновый сервис обработки бронирований.
/// Периодически получает из базы данных идентификаторы броней
/// в статусе Pending и обрабатывает их параллельно.
/// </summary>
public class BookingProcessingBackgroundService(IServiceScopeFactory scopeFactory, ILogger<BookingProcessingBackgroundService> logger) : BackgroundService
{
    private const int PollingDelayMs = 1_000;
    private const int MaxDegreeOfParallelism = 4;

    /// <summary>
    /// Выполняет непрерывную фоновую обработку бронирований
    /// до остановки приложения.
    /// </summary>
    /// <param name="stoppingToken">Токен остановки фонового сервиса.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var pendingBookingIds = await GetPendingBookingIdsAsync(stoppingToken);

                await Parallel.ForEachAsync(pendingBookingIds, new ParallelOptions
                {
                    CancellationToken = stoppingToken,
                    MaxDegreeOfParallelism = MaxDegreeOfParallelism
                },
                    ProcessBookingAsync);
                await Task.Delay(PollingDelayMs, stoppingToken);
            }
        }

        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Фоновая обработка бронирований остановлена.");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex,
                                "Фоновый сервис обработки бронирований завершился с ошибкой.");
            throw;
        }
    }
    /// <summary>
    /// Обрабатывает одну бронь в отдельном scope.
    /// </summary>
    private async Task<IReadOnlyList<Guid>> GetPendingBookingIdsAsync(CancellationToken stopingToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
        return await bookingRepository.GetPendingIdsAsync(stopingToken);
    }

    private async ValueTask ProcessBookingAsync(Guid bookingId, CancellationToken stoppingToken)
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
        catch (NotFoundException ex)
        {
            logger.LogWarning(ex,
                               "Бронирование {BookingId} не найдено во время фоновой обработки.",
                               bookingId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                             "Ошибка при обработке брони {BookingId}",
                             bookingId);
        }
    }
}

using EventFlow.Api.Services.Interfaces;

namespace EventFlow.Api.Services;

/// <summary>
/// Фоновый сервис обработки бронирований.
/// Получает идентификаторы созданных броней из очереди,
/// имитирует обращение к внешней системе и переводит бронь из Pending в Confirmed.
/// </summary>
public class BookingProcessingBackgroundService : BackgroundService
{
    private const int ProcessingDelayMs = 2_000;
    private const int MaxDegreeOfParallelism = 4;

    private readonly IBookingService _bookingService;
    private readonly IBookingTaskQueue _taskQueue;
    private readonly ILogger<BookingProcessingBackgroundService> _logger;


    public BookingProcessingBackgroundService(IBookingService bookingService, IBookingTaskQueue taskQueue, ILogger<BookingProcessingBackgroundService> logger)
    {
        _bookingService = bookingService;
        _taskQueue = taskQueue;
        _logger = logger;
    }
    /// <summary>
    /// Выполняет непрерывную фоновую обработку бронирований до отмены приложения.
    /// </summary>
    /// <param name="stoppingToken">Токен остановки фонового сервиса.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Parallel.ForEachAsync(
                _taskQueue.DequeueAllAsync(stoppingToken),
                new ParallelOptions
                {
                    CancellationToken = stoppingToken,
                    MaxDegreeOfParallelism = MaxDegreeOfParallelism
                },
                ProcessBookingAsync);
        }

        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Фоновая обработка бронирований остановлена.");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex,
                                "Фоновый сервис обработки бронирований завершился с ошибкой.");
            throw;
        }
    }

    private async ValueTask ProcessBookingAsync(Guid bookingId, CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(ProcessingDelayMs, stoppingToken);

            await _bookingService.ProcessBookingAsync(bookingId, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            throw;
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex,
                               "Бронирование {BookingId} не найдено во время фоновой обработки.",
                               bookingId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                             "Ошибка при обработке брони {BookingId}",
                             bookingId);
        }
    }
}

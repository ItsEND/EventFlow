using EventFlow.Api.Services.Interfaces;

namespace EventFlow.Api.Services;

/// <summary>
/// Фоновый сервис обработки бронирований.
/// Получает идентификаторы созданных броней из очереди,
/// имитирует обращение к внешней системе и переводит бронь из Pending в Confirmed.
/// </summary>
public class BookingProcessingBackgroundService : BackgroundService
{

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
            await foreach (var bookingId in _taskQueue.DequeueAllAsync(stoppingToken))
            {
                try
                {
                    await Task.Delay(2_000, stoppingToken);
                    await _bookingService.ProcessBookingAsync(bookingId, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обработке брони {BookingId}", bookingId);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Фоновая обработка бронирований остановлена");
        }
    }
}

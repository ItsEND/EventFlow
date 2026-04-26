using EventFlow.Api.Models;
using EventFlow.Api.Services.Interfaces;

namespace EventFlow.Api.Services
{
    public class BookingBackgroundCreateService : BackgroundService
    {

        private readonly IBookingService _bookingService;
        private readonly IBookingTaskQueue _taskQueue;

        private readonly ILogger<BookingBackgroundCreateService> _logger;

        public BookingBackgroundCreateService(IBookingService bookingService, IBookingTaskQueue taskQueue, ILogger<BookingBackgroundCreateService> logger)
        {
            _bookingService = bookingService;
            _taskQueue = taskQueue;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await foreach   (var bookingId in _taskQueue.DequeueAllAsync(stoppingToken))
                {
                    try
                    {
                        await Task.Delay(2000, stoppingToken);
                        await _bookingService.ProcessBookingAsync(bookingId,stoppingToken);
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
}

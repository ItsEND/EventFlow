using EventFlow.Api.Models;
using EventFlow.Api.Repositories.Interfaces;
using EventFlow.Api.Services.Interfaces;

namespace EventFlow.Api.Services;

/// <summary>
/// Сервис для создания, получения и обработки бронирований.
/// Сохраняет бронирования в базе данных и защищает критическую секцию
/// при резервировании мест.
/// </summary>
/// <remarks>
/// Инициализирует сервис бронирований.
/// </remarks>
/// <param name="eventService">Сервис для получения информации о мероприятиях.</param>
/// <param name="logger">Логгер сервиса бронирований.</param>
/// <param name="bookingRepository">Резпозиторий бронирования приложения.</param>
public class BookingService(IEventService eventService, ILogger<BookingService> logger, IBookingRepository bookingRepository) : IBookingService
{
    private static readonly SemaphoreSlim _bookingSemaphore = new(1, 1);

    /// <summary>
    /// Создаёт бронь для указанного мероприятия.
    /// Если свободные места есть, уменьшает AvailableSeats
    /// и создаёт бронь в статусе Pending.
    /// </summary>
    /// <param name="eventId">Идентификатор мероприятия.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Созданная бронь.</returns>
    /// <exception cref="NotFoundException">
    /// Выбрасывается, если мероприятие не найдено.
    /// </exception>
    /// <exception cref="NoAvailableSeatsException">
    /// Выбрасывается, если на мероприятие больше нет свободных мест.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Выбрасывается, если бронь не удалось сохранить.
    /// </exception>
    public async Task<Booking> CreateBookingAsync(Guid eventId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        Booking booking;
        Event ev;

        await _bookingSemaphore.WaitAsync(ct);
        try
        {
            ev = await eventService.GetEventAsync(eventId, ct);

            if (!ev.TryReserveSeats())
            {
                throw new NoAvailableSeatsException("Свободных мест на это мероприятие нет.");
            }

            try
            {
                booking = Booking.Create(eventId);

                bookingRepository.Add(booking);
                await bookingRepository.SaveChangesAsync(ct);
            }
            catch (OperationCanceledException)
            {
                ev.ReleaseSeats();
                throw;
            }
            catch (Exception ex)
            {
                ev.ReleaseSeats();

                throw new InvalidOperationException("Не удалось создать бронирование.", ex);
            }
            logger.LogInformation("Бронирование {BookingId} создано для события {EventId}. Доступных мест={AvailableSeats}",
                                   booking.Id,
                                   eventId,
                                   ev.AvailableSeats);
        }

        finally
        {
            _bookingSemaphore.Release();
        }

        return booking;
    }


    /// <summary>
    /// Возвращает бронь по её идентификатору.
    /// </summary>
    /// <param name="bookingId">Идентификатор брони.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Найденная бронь.</returns>
    /// <exception cref="NotFoundException">
    /// Выбрасывается, если бронь не найдена.
    /// </exception>
    public async Task<Booking> GetBookingByIdAsync(Guid bookingId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        return await bookingRepository.GetByIdAsync(bookingId, ct)
            ?? throw new NotFoundException("Booking", bookingId);
    }

    /// <summary>
    /// Обрабатывает бронь, полученную из фоновой очереди.
    /// Если бронь находится в статусе Pending и мероприятие существует,
    /// переводит бронь в статус Confirmed.
    /// Если мероприятие было удалено, переводит бронь в статус Rejected.
    /// </summary>
    /// <param name="bookingId">Идентификатор брони.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Обработанная бронь.</returns>
    /// <exception cref="NotFoundException">
    /// Выбрасывается, если бронь не найдена.
    /// </exception>
    public async Task<Booking> ProcessBookingAsync(Guid bookingId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var booking = await GetBookingByIdAsync(bookingId, ct);

        if (booking.Status != BookingStatus.Pending)
        {
            logger.LogInformation("Бронирование {BookingId} пропущено, поскольку оно уже обработано. Статус={Status}",
                                   booking.Id,
                                   booking.Status);

            return booking;
        }

        try
        {
            await eventService.GetEventAsync(booking.EventId, ct);
        }
        catch (NotFoundException)
        {
            booking.Reject();
            await bookingRepository.SaveChangesAsync(ct);

            logger.LogWarning("Бронирование {BookingId} отклонено, так как событие {EventId} не найдено.",
                               booking.Id,
                               booking.EventId);

            return booking;
        }

        booking.Confirm();
        await bookingRepository.SaveChangesAsync(ct);

        logger.LogInformation("Бронирование {BookingId} для события {EventId} подтверждено.",
                               booking.Id,
                               booking.EventId);

        return booking;
    }
}

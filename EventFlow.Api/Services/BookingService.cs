using EventFlow.Api.Models;
using EventFlow.Api.Services.Interfaces;
using System.Collections.Concurrent;

namespace EventFlow.Api.Services;

/// <summary>
/// Сервис для создания, получения и обработки бронирований.
/// Хранит бронирования в памяти приложения, защищает критические секции
/// при резервировании мест и передаёт созданные брони в очередь фоновой обработки.
/// </summary>
public class BookingService : IBookingService
{
    private readonly IEventService _eventService;
    private readonly IBookingTaskQueue _taskQueue;
    private readonly ILogger<BookingService> _logger;

    private readonly ConcurrentDictionary<Guid, Booking> _bookings;

    private readonly SemaphoreSlim _bookingSemaphore = new(1, 1);

    /// <summary>
    /// Инициализирует сервис бронирований.
    /// </summary>
    /// <param name="eventService">Сервис мероприятий.</param>
    /// <param name="taskQueue">Очередь фоновой обработки бронирований.</param>
    /// <param name="logger">Логгер сервиса бронирований.</param>
    /// <param name="bookings">Начальный набор бронирований.</param>
    public BookingService(IEventService eventService, IBookingTaskQueue taskQueue, ILogger<BookingService> logger, IEnumerable<Booking>? bookings = null)
    {
        _eventService = eventService;
        _taskQueue = taskQueue;
        _logger = logger;

        var initDict = bookings?.ToDictionary(b => b.Id, b => b) ?? [];
        _bookings = new ConcurrentDictionary<Guid, Booking>(initDict);
    }

    /// <summary>
    /// Создаёт бронь для указанного мероприятия.
    /// Если свободные места есть, уменьшает AvailableSeats, создаёт бронь
    /// в статусе Pending и добавляет её в очередь фоновой обработки.
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
            ev = await _eventService.GetEvent(eventId);

            if (!ev.TryReserveSeats())
            {
                throw new NoAvailableSeatsException("Свободных мест на это мероприятие нет.");
            }

            booking = Booking.Create(eventId);

            if (!_bookings.TryAdd(booking.Id, booking))
            {
                ev.ReleaseSeats();
                throw new InvalidOperationException("Не удалось создать бронирование.");
            }

            _logger.LogInformation("Бронирование {BookingId} создано для события {EventId}. Доступных мест={AvailableSeats}",
                                   booking.Id,
                                   eventId,
                                   ev.AvailableSeats);
        }

        finally
        {
            _bookingSemaphore.Release();
        }

        try
        {
            await _taskQueue.EnqueueAsync(booking.Id, ct);
        }
        catch (Exception ex)
        {
            await RollbackCreatedBookingAsync(booking.Id, ev, CancellationToken.None);

            _logger.LogError(ex,
                             "Failed to enqueue booking {BookingId}. Booking was rolled back.",
                             booking.Id);

            throw;
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
    public Task<Booking> GetBookingByIdAsync(Guid bookingId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        return _bookings.TryGetValue(bookingId, out var booking)
            ? Task.FromResult(booking)
            : throw new NotFoundException("Booking", bookingId);
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

        Booking booking;

        try
        {
            booking = await GetBookingByIdAsync(bookingId, ct);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex,
                               "Не удалось обработать бронирование {BookingId}: бронирование не найдено.",
                               bookingId);

            throw;
        }

        await _bookingSemaphore.WaitAsync(ct);

        try
        {
            if (booking.Status != BookingStatus.Pending)
            {
                _logger.LogInformation("Бронирование {BookingId} пропущено, поскольку оно уже обработано. Статус={Status}",
                                       booking.Id,
                                       booking.Status);

                return booking;
            }

            try
            {
                await _eventService.GetEvent(booking.EventId);
            }
            catch (NotFoundException)
            {
                booking.Reject();

                _logger.LogWarning("Бронирование {BookingId} отклонено, так как событие {EventId} не найдено.",
                                   booking.Id,
                                   booking.EventId);

                return booking;
            }

            booking.Confirm();

            _logger.LogInformation("Бронирование {BookingId} для события {EventId} подтверждено.",
                                   booking.Id,
                                   booking.EventId);

            return booking;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await RejectBookingAndReleaseSeatSafelyAsync(booking, ex);
            return booking;
        }
        finally
        {
            _bookingSemaphore.Release();
        }
    }

    /// <summary>
    /// Выполняет откат созданной брони, если её не удалось добавить
    /// в очередь фоновой обработки.
    /// Удаляет бронь из хранилища и возвращает зарезервированное место.
    /// </summary>
    /// <param name="bookingId">Идентификатор брони.</param>
    /// <param name="ev">Мероприятие, для которого была создана бронь.</param>
    /// <param name="ct">Токен отмены операции.</param>
    private async Task RollbackCreatedBookingAsync(Guid bookingId, Event ev, CancellationToken ct)
    {
        await _bookingSemaphore.WaitAsync(ct);
        try
        {
            if (_bookings.TryRemove(bookingId, out _))
            {
                ev.ReleaseSeats();

                _logger.LogWarning("Бронирование {BookingId} было удалено, и место освободилось. Доступно мест={AvailableSeats}",
                                   bookingId,
                                   ev.AvailableSeats);
            }
        }
        finally
        {
            _bookingSemaphore.Release();
        }
    }

    /// <summary>
    /// Безопасно отклоняет бронь после непредвиденной ошибки обработки.
    /// Если связанное мероприятие существует, возвращает место в пул доступных мест.
    /// Если мероприятие удалено, отклоняет бронь без возврата места.
    /// </summary>
    /// <param name="booking">Бронь, которую нужно отклонить.</param>
    /// <param name="originalException">Изначальная ошибка, из-за которой выполняется отклонение.</param>
    private async Task RejectBookingAndReleaseSeatSafelyAsync(Booking booking, Exception originalException)
    {
        if (booking.Status != BookingStatus.Pending)
        {
            _logger.LogError(originalException,
                             "Непредвиденная ошибка при обработке бронирования {BookingId}, но бронирование уже обработано. Статус={Status}",
                             booking.Id,
                             booking.Status);

            return;
        }

        try
        {
            var ev = await _eventService.GetEvent(booking.EventId);

            booking.Reject();
            ev.ReleaseSeats();

            _logger.LogError(originalException,
                             "Бронирование {BookingId} отклонено из-за непредвиденной ошибки. Место возвращено для события {EventId}. Доступно мест={AvailableSeats}",
                             booking.Id,
                             booking.EventId,
                             ev.AvailableSeats);
        }
        catch (NotFoundException)
        {
            booking.Reject();

            _logger.LogError(originalException,
                             "Бронирование {BookingId} отклонено из-за непредвиденной ошибки, но событие {EventId} не найдено. Место вернуть невозможно.",
                             booking.Id,
                             booking.EventId);
        }
        catch (Exception rollbackException)
        {
            _logger.LogCritical(rollbackException,
                                "Не удалось безопасно отклонить бронирование {BookingId} после первоначальной ошибки: {OriginalError}",
                                booking.Id,
                                originalException.Message);

            throw;
        }

    }

}


using EventFlow.Events.Application.Abstractions.Repositories;
using EventFlow.Events.Application.Abstractions.Services;
using EventFlow.Events.Application.Dtos.Booking;
using EventFlow.Events.Application.Exceptions;
using EventFlow.Events.Domain.Exceptions;
using EventFlow.Events.Domain.Models;

namespace EventFlow.Events.Application.Services;

/// <summary>
/// Сервис для создания, получения и обработки бронирований.
/// </summary>
public class BookingService(
    IEventRepository eventRepository,
    IBookingRepository bookingRepository,
    IBookingTaskQueue bookingTaskQueue) : IBookingService
{
    private static readonly SemaphoreSlim BookingSemaphore = new(1, 1);
    private const int MaxActiveBookingsPerUser = 10;

    public async Task<BookingDto> CreateBookingAsync(Guid eventId, Guid userId, CancellationToken ct)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            Booking booking;
            Event ev;

            await BookingSemaphore.WaitAsync(ct);
            try
            {
                ev = await eventRepository.GetByIdAsync(eventId, ct)
                    ?? throw new NotFoundException("Event", eventId);

                if (ev.StartAt <= DateTime.UtcNow)
                {
                    throw new EventAlreadyStartedException("Нельзя забронировать событие, которое уже началось.");
                }

                var activeBookingsCount = await bookingRepository.CountActiveByUserIdAsync(userId, ct);

                if (activeBookingsCount >= MaxActiveBookingsPerUser)
                {
                    throw new BookingLimitExceededException(
                        $"Пользователь не может иметь больше " +
                        $"{MaxActiveBookingsPerUser} активных бронирований.");
                }

                ev.ReserveSeats();

                try
                {
                    booking = Booking.Create(eventId, userId);

                    bookingRepository.Add(booking);
                    await bookingRepository.SaveChangesAsync(ct);
                    await bookingTaskQueue.EnqueueAsync(booking.Id, CancellationToken.None);
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

                return MapToDto(booking);
            }
            finally
            {
                BookingSemaphore.Release();
            }
        }
        catch (NotFoundException ex)
        {
            throw AppException.NotFound(ex.Message, ex);
        }
        catch (NoAvailableSeatsException ex)
        {
            throw AppException.NoAvailableSeats(ex.Message, ex);
        }
        catch (EventAlreadyStartedException ex)
        {
            throw AppException.EventAlreadyStarted(ex.Message, ex);
        }
        catch (BookingLimitExceededException ex)
        {
            throw AppException.BookingLimitExceeded(ex.Message, ex);
        }
    }

    public async Task<BookingDto> GetBookingByIdAsync(Guid bookingId, CancellationToken ct)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            var booking = await bookingRepository.GetByIdAsync(bookingId, ct)
                ?? throw new NotFoundException("Booking", bookingId);

            return MapToDto(booking);
        }
        catch (NotFoundException ex)
        {
            throw AppException.NotFound(ex.Message, ex);
        }
    }

    public async Task<BookingDto> ProcessBookingAsync(Guid bookingId, CancellationToken ct)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            var booking = await bookingRepository.GetByIdAsync(bookingId, ct)
                ?? throw new NotFoundException("Booking", bookingId);

            if (booking.Status != BookingStatus.Pending)
            {
                return MapToDto(booking);
            }

            var eventExists = await eventRepository.GetByIdAsync(booking.EventId, ct) is not null;

            if (!eventExists)
            {
                booking.Reject();
                await bookingRepository.SaveChangesAsync(ct);
                return MapToDto(booking);
            }

            booking.Confirm();
            await bookingRepository.SaveChangesAsync(ct);
            return MapToDto(booking);
        }
        catch (NotFoundException ex)
        {
            throw AppException.NotFound(ex.Message, ex);
        }
    }

    private static BookingDto MapToDto(Booking booking) => new()
    {
        Id = booking.Id,
        EventId = booking.EventId,
        UserId = booking.UserId,
        Status = booking.Status.ToString(),
        CreatedAt = booking.CreatedAt,
        ProcessedAt = booking.ProcessedAt
    };

    public async Task CancelBookingAsync(Guid bookingId, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var booking = await bookingRepository.GetByIdAsync(bookingId, cancellationToken)
                ?? throw new NotFoundException("Booking", bookingId);
            if (!isAdmin && booking.UserId != currentUserId)
            {
                throw new ForbiddenOperationException("Пользователь может отменить только свою бронь");
            }

            var ev = await eventRepository.GetByIdAsync(booking.EventId, cancellationToken)
                ?? throw new NotFoundException("Event", booking.EventId);

            booking.Cancel();

            ev.ReleaseSeats();

            await bookingRepository.SaveChangesAsync(cancellationToken);
        }
        catch (NotFoundException ex)
        {
            throw AppException.NotFound(ex.Message, ex);
        }
        catch (ForbiddenOperationException ex)
        {
            throw AppException.Forbidden(ex.Message, ex);
        }
    }
}

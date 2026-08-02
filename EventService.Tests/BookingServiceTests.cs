using EventFlow.Events.Application.Abstractions.Services;
using EventFlow.Events.Application.Dtos.Booking;
using EventFlow.Events.Application.Exceptions;
using EventFlow.Events.Domain.Models;
using EventFlow.Events.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace EventService.Tests;

public class BookingServiceTests : IDisposable
{
    private readonly ServiceProvider _provider;
    private readonly IServiceScope _scope;
    private readonly IEventService _eventService;
    private readonly IBookingService _bookingService;
    private readonly List<Event> _seedEvents;
    private readonly Guid _userId = Guid.NewGuid();
    public BookingServiceTests()
    {
        _provider = TestServiceProviderFactory.Create();

        _seedEvents = SeedEvents();
        AddEvents(_seedEvents);

        _scope = _provider.CreateScope();

        _eventService = _scope.ServiceProvider.GetRequiredService<IEventService>();
        _bookingService = _scope.ServiceProvider.GetRequiredService<IBookingService>();
    }

    public void Dispose()
    {
        _scope.Dispose();
        _provider.Dispose();
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldCreatePendingBooking_WhenEventExists()
    {
        var eventId = _seedEvents.First().Id;
        var booking = await _bookingService.CreateBookingAsync(eventId, _userId, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, booking.Id);
        Assert.Equal(eventId, booking.EventId);
        Assert.Equal(_userId, booking.UserId);
        Assert.Equal(BookingStatus.Pending.ToString(), booking.Status);
        Assert.NotEqual(default, booking.CreatedAt);
        Assert.Null(booking.ProcessedAt);
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldCreateSeveralBookingsWithUniqueIds_WhenSameEventUsed()
    {
        var eventId = _seedEvents.First().Id;

        var firstBooking = await _bookingService.CreateBookingAsync(eventId, _userId, CancellationToken.None);
        var secondBooking = await _bookingService.CreateBookingAsync(eventId, _userId, CancellationToken.None);

        Assert.Equal(eventId, firstBooking.EventId);
        Assert.Equal(eventId, secondBooking.EventId);

        Assert.NotEqual(Guid.Empty, firstBooking.Id);
        Assert.NotEqual(Guid.Empty, secondBooking.Id);
        Assert.NotEqual(firstBooking.Id, secondBooking.Id);

        Assert.Equal(BookingStatus.Pending.ToString(), firstBooking.Status);
        Assert.Equal(BookingStatus.Pending.ToString(), secondBooking.Status);
    }

    [Fact]
    public async Task GetBookingByIdAsync_ShouldReturnBooking_WhenBookingExists()
    {
        var eventId = _seedEvents.First().Id;
        var createdBooking = await _bookingService.CreateBookingAsync(eventId, _userId, CancellationToken.None);

        var foundBooking = await _bookingService.GetBookingByIdAsync(createdBooking.Id, CancellationToken.None);

        Assert.Equal(createdBooking.Id, foundBooking.Id);
        Assert.Equal(createdBooking.EventId, foundBooking.EventId);
        Assert.Equal(createdBooking.Status, foundBooking.Status);
        Assert.Equal(createdBooking.CreatedAt, foundBooking.CreatedAt);
        Assert.Equal(createdBooking.ProcessedAt, foundBooking.ProcessedAt);
    }

    [Fact]
    public async Task ProcessBookingAsync_ShouldChangeStatusToConfirmedAndSetProcessedAt()
    {
        var eventId = _seedEvents.First().Id;
        var createdBooking = await _bookingService.CreateBookingAsync(eventId, _userId, CancellationToken.None);

        var processedBooking = await _bookingService.ProcessBookingAsync(createdBooking.Id, CancellationToken.None);

        Assert.Equal(createdBooking.Id, processedBooking.Id);
        Assert.Equal(BookingStatus.Confirmed.ToString(), processedBooking.Status);
        Assert.NotNull(processedBooking.ProcessedAt);
    }

    [Fact]
    public async Task GetBookingByIdAsync_ShouldReflectStatusChange_AfterProcessing()
    {
        var eventId = _seedEvents.First().Id;
        var createdBooking = await _bookingService.CreateBookingAsync(eventId, _userId, CancellationToken.None);

        await _bookingService.ProcessBookingAsync(createdBooking.Id, CancellationToken.None);
        var foundBooking = await _bookingService.GetBookingByIdAsync(createdBooking.Id, CancellationToken.None);

        Assert.Equal(BookingStatus.Confirmed.ToString(), foundBooking.Status);
        Assert.NotNull(foundBooking.ProcessedAt);
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldThrowAppExceptionWithNotFoundCode_WhenEventDoesNotExist()
    {
        var nonExistingEventId = Guid.NewGuid();
        var action = async () => await _bookingService.CreateBookingAsync(nonExistingEventId, _userId, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<AppException>(action);
        Assert.Equal(AppErrorCode.NotFound, exception.Code);
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldThrowAppExceptionWithNotFoundCode_WhenEventWasDeleted()
    {
        var eventId = _seedEvents.First().Id;
        await _eventService.RemoveEventAsync(eventId, CancellationToken.None);

        var action = async () => await _bookingService.CreateBookingAsync(eventId, _userId, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<AppException>(action);
        Assert.Equal(AppErrorCode.NotFound, exception.Code);
    }

    [Fact]
    public async Task GetBookingByIdAsync_ShouldThrowAppExceptionWithNotFoundCode_WhenBookingDoesNotExist()
    {
        var nonExistingBookingId = Guid.NewGuid();
        var action = async () => await _bookingService.GetBookingByIdAsync(nonExistingBookingId, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<AppException>(action);
        Assert.Equal(AppErrorCode.NotFound, exception.Code);
    }

    [Fact]
    public async Task ProcessBookingAsync_ShouldThrowAppExceptionWithNotFoundCode_WhenBookingDoesNotExist()
    {
        var nonExistingBookingId = Guid.NewGuid();
        var action = async () => await _bookingService.ProcessBookingAsync(nonExistingBookingId, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<AppException>(action);
        Assert.Equal(AppErrorCode.NotFound, exception.Code);
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldDecreaseAvailableSeatsByOne()
    {
        var eventId = _seedEvents.First().Id;

        var before = await GetAvailableSeatsAsync(eventId);

        await _bookingService.CreateBookingAsync(eventId, _userId, CancellationToken.None);

        var after = await GetAvailableSeatsAsync(eventId);

        Assert.Equal(before - 1, after);
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldThrowAppExceptionWithNoAvailableSeatsCode_WhenNoSeatsLeft()
    {
        var ev = Event.Create(
            "Small event",
            null,
            1,
            new DateTime(2030, 6, 1, 10, 0, 0),
            new DateTime(2030, 6, 1, 12, 0, 0));

        AddEvent(ev);

        await _bookingService.CreateBookingAsync(ev.Id, _userId, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<AppException>(() =>
            _bookingService.CreateBookingAsync(ev.Id, _userId, CancellationToken.None));
        Assert.Equal(AppErrorCode.NoAvailableSeats, exception.Code);

        var availableSeats = await GetAvailableSeatsAsync(ev.Id);

        Assert.Equal(0, availableSeats);
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldPreventOverbooking_WhenManyConcurrentRequests()
    {
        var ev = Event.Create(
            "Limited event",
            null,
            5,
            new DateTime(2030, 6, 1, 10, 0, 0),
            new DateTime(2030, 6, 1, 12, 0, 0));

        AddEvent(ev);

        const int concurrentRequests = 20;

        var tasks = Enumerable.Range(0, concurrentRequests)
            .Select(_ => Task.Run(async () =>
            {
                using var scope = _provider.CreateScope();
                var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                try
                {
                    var booking = await bookingService.CreateBookingAsync(ev.Id, _userId, CancellationToken.None);
                    return (Success: true, Booking: booking, Exception: (Exception?)null);
                }
                catch (Exception ex)
                {
                    return (Success: false, Booking: (BookingDto?)null, Exception: ex);
                }
            }))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        var successful = results.Where(result => result.Success).ToList();
        var failed = results.Where(result => !result.Success).ToList();

        Assert.Equal(5, successful.Count);
        Assert.Equal(15, failed.Count);

        Assert.All(failed, result =>
        {
            var exception = Assert.IsType<AppException>(result.Exception);
            Assert.Equal(AppErrorCode.NoAvailableSeats, exception.Code);
        });

        Assert.Equal(
            successful.Count,
            successful.Select(result => result.Booking!.Id).Distinct().Count());

        Assert.Equal(0, await GetAvailableSeatsAsync(ev.Id));
        Assert.Equal(5, await GetBookingCountAsync(ev.Id));
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldCreateUniqueBookingIds_WhenConcurrentRequests()
    {
        var ev = Event.Create(
            "Concurrent event",
            null,
            10,
            new DateTime(2030, 6, 1, 10, 0, 0),
            new DateTime(2030, 6, 1, 12, 0, 0));

        AddEvent(ev);

        const int concurrentRequests = 10;

        var tasks = Enumerable.Range(0, concurrentRequests)
            .Select(_ => Task.Run(async () =>
            {
                using var scope = _provider.CreateScope();
                var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                return await bookingService.CreateBookingAsync(ev.Id, _userId, CancellationToken.None);
            }))
            .ToArray();

        var bookings = await Task.WhenAll(tasks);

        Assert.Equal(10, bookings.Length);
        Assert.Equal(10, bookings.Select(booking => booking.Id).Distinct().Count());
        Assert.Equal(0, await GetAvailableSeatsAsync(ev.Id));
        Assert.Equal(10, await GetBookingCountAsync(ev.Id));
    }

    [Fact]
    public async Task Cancel_ShouldChangeBookingStatusToCancelled()
    {
        var eventId = Guid.NewGuid();
        var booking = Booking.Create(eventId, _userId);

        booking.Cancel();

        Assert.Equal(BookingStatus.Cancelled, booking.Status);
        Assert.NotNull(booking.ProcessedAt);
    }

    [Fact]
    public async Task Cancel_ShouldThrowValidationException_WhenBookingAlreadyCancelled()
    {
        var eventId = Guid.NewGuid();
        var booking = Booking.Create(eventId, _userId);

        booking.Cancel();

        Assert.Throws<ValidationException>(() => booking.Cancel());
    }

    [Fact]
    public void ReleaseSeats_ShouldRestoreSeat_AfterBookingRejected()
    {
        var ev = Event.Create(
            "Event",
            null,
            1,
            new DateTime(2030, 6, 1, 10, 0, 0),
            new DateTime(2030, 6, 1, 12, 0, 0));

        var booking = Booking.Create(ev.Id, _userId);

        var reserved = ev.TryReserveSeats();
        booking.Reject();
        ev.ReleaseSeats();

        Assert.True(reserved);
        Assert.Equal(BookingStatus.Rejected, booking.Status);
        Assert.Equal(1, ev.AvailableSeats);
    }

    [Fact]
    public void TryReserveSeats_ShouldAllowNewReservation_AfterReleaseSeats()
    {
        var ev = Event.Create(
            "Event",
            null,
            1,
            new DateTime(2026, 6, 1, 10, 0, 0),
            new DateTime(2026, 6, 1, 12, 0, 0));

        Assert.True(ev.TryReserveSeats());
        Assert.Equal(0, ev.AvailableSeats);

        ev.ReleaseSeats();

        Assert.True(ev.TryReserveSeats());
        Assert.Equal(0, ev.AvailableSeats);
    }
    [Fact]
    public async Task CreateBookingAsync_ShouldFail_WhenEventAlreadyStarted()
    {
        var pastEvent = Event.Create("Past event", null, 10, DateTime.UtcNow.AddHours(-2), DateTime.UtcNow.AddHours(-1));

        AddEvent(pastEvent);

        var exception = await Assert.ThrowsAsync<AppException>(
            () => _bookingService.CreateBookingAsync(
                pastEvent.Id,
                _userId,
                CancellationToken.None));

        Assert.Equal(AppErrorCode.EventAlreadyStarted, exception.Code);
    }
    [Fact]
    public async Task CreateBookingAsync_ShouldUseSeparateLimitsForDifferentUsers()
    {
        var ev = Event.Create("Event with separate user limits", null, 20, DateTime.UtcNow.AddDays(10), DateTime.UtcNow.AddDays(11));

        AddEvent(ev);

        var firstUserId = Guid.NewGuid();
        var secondUserId = Guid.NewGuid();

        for (var index = 0; index < 10; index++)
        {
            await _bookingService.CreateBookingAsync(ev.Id, firstUserId, CancellationToken.None);
        }

        await Assert.ThrowsAsync<AppException>(() => _bookingService.CreateBookingAsync(ev.Id, firstUserId, CancellationToken.None));

        var secondUserBooking = await _bookingService.CreateBookingAsync(ev.Id, secondUserId, CancellationToken.None);

        Assert.Equal(secondUserId, secondUserBooking.UserId);
        Assert.Equal(BookingStatus.Pending.ToString(), secondUserBooking.Status);
    }
    [Fact]
    public async Task CreateBookingAsync_ShouldFail_WhenUserHasTenActiveBookings()
    {
        var ev = Event.Create("Large event", null, 20, DateTime.UtcNow.AddDays(10), DateTime.UtcNow.AddDays(11));

        AddEvent(ev);

        for (var index = 0; index < 10; index++)
        {
            await _bookingService.CreateBookingAsync(ev.Id, _userId, CancellationToken.None);
        }

        var exception = await Assert.ThrowsAsync<AppException>(
            () => _bookingService.CreateBookingAsync(
                ev.Id,
                _userId,
                CancellationToken.None));

        Assert.Equal(AppErrorCode.BookingLimitExceeded, exception.Code);

        Assert.Contains("10", exception.Message);
    }

    private void AddEvents(IEnumerable<Event> events)
    {
        using var scope = _provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        context.Events.AddRange(events);
        context.SaveChanges();
    }

    private void AddEvent(Event ev)
    {
        using var scope = _provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        context.Events.Add(ev);
        context.SaveChanges();
    }

    private async Task<int> GetAvailableSeatsAsync(Guid eventId)
    {
        await using var scope = _provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await context.Events
            .AsNoTracking()
            .Where(ev => ev.Id == eventId)
            .Select(ev => ev.AvailableSeats)
            .SingleAsync();
    }

    private async Task<int> GetBookingCountAsync(Guid eventId)
    {
        await using var scope = _provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await context.Bookings.CountAsync(booking => booking.EventId == eventId);
    }

    [Fact]
    public async Task CancelBookingAsync_ShouldCancelOwnBooking()
    {
        var eventId = _seedEvents.First().Id;

        var booking = await _bookingService.CreateBookingAsync(eventId, _userId, CancellationToken.None);

        await _bookingService.CancelBookingAsync(booking.Id, _userId, isAdmin: false, CancellationToken.None);

        var cancelledBooking =
            await _bookingService.GetBookingByIdAsync(booking.Id, CancellationToken.None);

        Assert.Equal(BookingStatus.Cancelled.ToString(), cancelledBooking.Status);
    }

    [Fact]
    public async Task CancelBookingAsync_ShouldReleaseEventSeat()
    {
        var eventId = _seedEvents.First().Id;

        var seatsBefore = await GetAvailableSeatsAsync(eventId);

        var booking = await _bookingService.CreateBookingAsync(eventId, _userId, CancellationToken.None);

        await _bookingService.CancelBookingAsync(booking.Id, _userId, isAdmin: false, CancellationToken.None);

        var seatsAfter = await GetAvailableSeatsAsync(eventId);

        Assert.Equal(seatsBefore, seatsAfter);
    }

    [Fact]
    public async Task CancelBookingAsync_ShouldForbidCancellingAnotherUsersBooking()
    {
        var eventId = _seedEvents.First().Id;

        var booking = await _bookingService.CreateBookingAsync(eventId, _userId, CancellationToken.None);

        var anotherUserId = Guid.NewGuid();

        var exception = await Assert.ThrowsAsync<AppException>(
            () => _bookingService.CancelBookingAsync(
                booking.Id,
                anotherUserId,
                isAdmin: false,
                CancellationToken.None));

        Assert.Equal(AppErrorCode.Forbidden, exception.Code);
    }
    [Fact]
    public async Task CancelBookingAsync_ShouldAllowAdminToCancelAnyBooking()
    {
        var eventId = _seedEvents.First().Id;

        var booking = await _bookingService.CreateBookingAsync(eventId, _userId, CancellationToken.None);

        var adminUserId = Guid.NewGuid();

        await _bookingService.CancelBookingAsync(booking.Id, adminUserId, isAdmin: true, CancellationToken.None);

        var cancelledBooking = await _bookingService.GetBookingByIdAsync(booking.Id, CancellationToken.None);

        Assert.Equal(BookingStatus.Cancelled.ToString(), cancelledBooking.Status);
    }
    private static List<Event> SeedEvents()
    {
        return
        [
            Event.Create(
                "Конференция .NET Backend",
                "Практики построения Web API на ASP.NET Core",
                10,
                new DateTime(2030, 4, 15, 10, 0, 0),
                new DateTime(2030, 4, 15, 18, 0, 0)),

            Event.Create(
                "Митап C# Junior",
                "Разбор базовых возможностей языка C#",
                10,
                new DateTime(2030, 4, 16, 18, 30, 0),
                new DateTime(2030, 4, 16, 20, 30, 0))
        ];
    }
}

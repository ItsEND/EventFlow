using EventFlow.Api;
using EventFlow.Api.Models;
using EventFlow.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace EventService.Tests;

public class BookingServiceTests
{
    private readonly EventFlow.Api.Services.EventService _eventService;
    private readonly BookingService _bookingService;
    private readonly InMemoryBookingTaskQueue _bookingTaskQueue;
    private readonly List<Event> _seedEvents;

    public BookingServiceTests()
    {
        _seedEvents = SeedEvents();

        var eventStore = new InMemoryEventStore(_seedEvents);

        _eventService = new EventFlow.Api.Services.EventService(eventStore);
        _bookingTaskQueue = new InMemoryBookingTaskQueue();

        _bookingService = new BookingService(_eventService, _bookingTaskQueue, NullLogger<BookingService>.Instance);
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldCreatePendingBooking_WhenEventExists()
    {
        // Arrange
        var eventId = _seedEvents.First().Id;

        // Act
        var booking = await _bookingService.CreateBookingAsync(eventId, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, booking.Id);
        Assert.Equal(eventId, booking.EventId);
        Assert.Equal(BookingStatus.Pending, booking.Status);
        Assert.NotEqual(default, booking.CreatedAt);
        Assert.Null(booking.ProcessedAt);
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldCreateSeveralBookingsWithUniqueIds_WhenSameEventUsed()
    {
        // Arrange
        var eventId = _seedEvents.First().Id;

        // Act
        var firstBooking = await _bookingService.CreateBookingAsync(eventId, CancellationToken.None);
        var secondBooking = await _bookingService.CreateBookingAsync(eventId, CancellationToken.None);

        // Assert
        Assert.Equal(eventId, firstBooking.EventId);
        Assert.Equal(eventId, secondBooking.EventId);

        Assert.NotEqual(Guid.Empty, firstBooking.Id);
        Assert.NotEqual(Guid.Empty, secondBooking.Id);
        Assert.NotEqual(firstBooking.Id, secondBooking.Id);

        Assert.Equal(BookingStatus.Pending, firstBooking.Status);
        Assert.Equal(BookingStatus.Pending, secondBooking.Status);
    }

    [Fact]
    public async Task GetBookingByIdAsync_ShouldReturnBooking_WhenBookingExists()
    {
        // Arrange
        var eventId = _seedEvents.First().Id;
        var createdBooking = await _bookingService.CreateBookingAsync(eventId, CancellationToken.None);

        // Act
        var foundBooking = await _bookingService.GetBookingByIdAsync(createdBooking.Id, CancellationToken.None);

        // Assert
        Assert.Equal(createdBooking.Id, foundBooking.Id);
        Assert.Equal(createdBooking.EventId, foundBooking.EventId);
        Assert.Equal(createdBooking.Status, foundBooking.Status);
        Assert.Equal(createdBooking.CreatedAt, foundBooking.CreatedAt);
        Assert.Equal(createdBooking.ProcessedAt, foundBooking.ProcessedAt);
    }

    [Fact]
    public async Task ProcessBookingAsync_ShouldChangeStatusToConfirmedAndSetProcessedAt()
    {
        // Arrange
        var eventId = _seedEvents.First().Id;
        var createdBooking = await _bookingService.CreateBookingAsync(eventId, CancellationToken.None);

        // Act
        var processedBooking = await _bookingService.ProcessBookingAsync(createdBooking.Id, CancellationToken.None);

        // Assert
        Assert.Equal(createdBooking.Id, processedBooking.Id);
        Assert.Equal(BookingStatus.Confirmed, processedBooking.Status);
        Assert.NotNull(processedBooking.ProcessedAt);
    }

    [Fact]
    public async Task GetBookingByIdAsync_ShouldReflectStatusChange_AfterProcessing()
    {
        // Arrange
        var eventId = _seedEvents.First().Id;
        var createdBooking = await _bookingService.CreateBookingAsync(eventId, CancellationToken.None);

        await _bookingService.ProcessBookingAsync(createdBooking.Id, CancellationToken.None);

        // Act
        var foundBooking = await _bookingService.GetBookingByIdAsync(createdBooking.Id, CancellationToken.None);

        // Assert
        Assert.Equal(BookingStatus.Confirmed, foundBooking.Status);
        Assert.NotNull(foundBooking.ProcessedAt);
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldThrowNotFoundException_WhenEventDoesNotExist()
    {
        // Arrange
        var nonExistingEventId = Guid.NewGuid();

        // Act
        var action = async () =>
            await _bookingService.CreateBookingAsync(nonExistingEventId, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(action);
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldThrowNotFoundException_WhenEventWasDeleted()
    {
        // Arrange
        var eventId = _seedEvents.First().Id;
        await _eventService.RemoveEvent(eventId);

        // Act
        var action = async () =>
            await _bookingService.CreateBookingAsync(eventId, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(action);
    }

    [Fact]
    public async Task GetBookingByIdAsync_ShouldThrowNotFoundException_WhenBookingDoesNotExist()
    {
        // Arrange
        var nonExistingBookingId = Guid.NewGuid();

        // Act
        var action = async () =>
            await _bookingService.GetBookingByIdAsync(nonExistingBookingId, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(action);
    }

    [Fact]
    public async Task ProcessBookingAsync_ShouldThrowNotFoundException_WhenBookingDoesNotExist()
    {
        // Arrange
        var nonExistingBookingId = Guid.NewGuid();

        // Act
        var action = async () =>
            await _bookingService.ProcessBookingAsync(nonExistingBookingId, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(action);
    }
    [Fact]
    public async Task CreateBookingAsync_ShouldDecreaseAvailableSeatsByOne()
    {
        var ev = _seedEvents.First();
        var before = ev.AvailableSeats;

        await _bookingService.CreateBookingAsync(ev.Id, CancellationToken.None);

        Assert.Equal(before - 1, ev.AvailableSeats);
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldThrowNoAvailableSeatsException_WhenNoSeatsLeft()
    {
        var ev = Event.Create(
            "Small event",
            null,
            1,
            new DateTime(2026, 6, 1, 10, 0, 0),
            new DateTime(2026, 6, 1, 12, 0, 0));

        var store = new InMemoryEventStore([ev]);
        var eventService = new EventFlow.Api.Services.EventService(store);
        var queue = new InMemoryBookingTaskQueue();
        var logger = NullLogger<BookingService>.Instance;

        var bookingService = new BookingService(eventService, queue, logger);

        await bookingService.CreateBookingAsync(ev.Id, CancellationToken.None);

        await Assert.ThrowsAsync<NoAvailableSeatsException>(() =>
            bookingService.CreateBookingAsync(ev.Id, CancellationToken.None));

        Assert.Equal(0, ev.AvailableSeats);
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldPreventOverbooking_WhenManyConcurrentRequests()
    {
        var ev = Event.Create(
            "Limited event",
            null,
            5,
            new DateTime(2026, 6, 1, 10, 0, 0),
            new DateTime(2026, 6, 1, 12, 0, 0));

        var store = new InMemoryEventStore([ev]);
        var eventService = new EventFlow.Api.Services.EventService(store);
        var queue = new InMemoryBookingTaskQueue();
        var bookingService = new BookingService(
            eventService,
            queue,
            NullLogger<BookingService>.Instance);

        var tasks = Enumerable.Range(0, 20)
            .Select(_ => Task.Run(async () =>
            {
                try
                {
                    var booking = await bookingService.CreateBookingAsync(ev.Id, CancellationToken.None);
                    return (Success: true, Booking: booking, Exception: (Exception?)null);
                }
                catch (Exception ex)
                {
                    return (Success: false, Booking: (Booking?)null, Exception: ex);
                }
            }))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        var successful = results.Where(x => x.Success).ToList();
        var failed = results.Where(x => !x.Success).ToList();

        Assert.Equal(5, successful.Count);
        Assert.Equal(15, failed.Count);
        Assert.All(failed, x => Assert.IsType<NoAvailableSeatsException>(x.Exception));
        Assert.Equal(0, ev.AvailableSeats);
        Assert.Equal(successful.Count, successful.Select(x => x.Booking!.Id).Distinct().Count());
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldCreateUniqueBookingIds_WhenConcurrentRequests()
    {
        var ev = Event.Create(
            "Concurrent event",
            null,
            10,
            new DateTime(2026, 6, 1, 10, 0, 0),
            new DateTime(2026, 6, 1, 12, 0, 0));

        var store = new InMemoryEventStore([ev]);
        var eventService = new EventFlow.Api.Services.EventService(store);
        var queue = new InMemoryBookingTaskQueue();
        var bookingService = new BookingService(
            eventService,
            queue,
            NullLogger<BookingService>.Instance);

        var tasks = Enumerable.Range(0, 10)
            .Select(_ => Task.Run(() =>
                bookingService.CreateBookingAsync(ev.Id, CancellationToken.None)))
            .ToArray();

        var bookings = await Task.WhenAll(tasks);

        Assert.Equal(10, bookings.Length);
        Assert.Equal(10, bookings.Select(x => x.Id).Distinct().Count());
        Assert.Equal(0, ev.AvailableSeats);
    }
    [Fact]
    public void ReleaseSeats_ShouldRestoreSeat_AfterBookingRejected()
    {
        var ev = Event.Create(
            "Event",
            null,
            1,
            new DateTime(2026, 6, 1, 10, 0, 0),
            new DateTime(2026, 6, 1, 12, 0, 0));

        var booking = Booking.Create(ev.Id);

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

    private static List<Event> SeedEvents()
    {
        return
        [
            Event.Create(
                "Конференция .NET Backend",
                "Практики построения Web API на ASP.NET Core",
                10,
                new DateTime(2026, 4, 15, 10, 0, 0),
                new DateTime(2026, 4, 15, 18, 0, 0)),

            Event.Create(
                "Митап C# Junior",
                "Разбор базовых возможностей языка C#",
                10,
                new DateTime(2026, 4, 16, 18, 30, 0),
                new DateTime(2026, 4, 16, 20, 30, 0))
        ];
    }
}
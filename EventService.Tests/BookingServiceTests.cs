using EventFlow.Api;
using EventFlow.Api.Models;
using EventFlow.Api.Services;

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
        _eventService = new EventFlow.Api.Services.EventService(_seedEvents);
        _bookingTaskQueue = new InMemoryBookingTaskQueue();
        _bookingService = new BookingService(_eventService, _bookingTaskQueue);
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
        _eventService.RemoveEvent(eventId);

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

    private static List<Event> SeedEvents()
    {
        return
        [
            Event.Create(
                "Конференция .NET Backend",
                "Практики построения Web API на ASP.NET Core",
                new DateTime(2026, 4, 15, 10, 0, 0),
                new DateTime(2026, 4, 15, 18, 0, 0)),

            Event.Create(
                "Митап C# Junior",
                "Разбор базовых возможностей языка C#",
                new DateTime(2026, 4, 16, 18, 30, 0),
                new DateTime(2026, 4, 16, 20, 30, 0))
        ];
    }
}
using EventFlow.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace EventService.Tests;

public class BookingTests
{
    [Fact]
    public void Create_ShouldCreateBookingWithPendingStatus()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        // Act
        var booking = Booking.Create(eventId);

        // Assert
        Assert.NotEqual(Guid.Empty, booking.Id);
        Assert.Equal(eventId, booking.EventId);
        Assert.Equal(BookingStatus.Pending, booking.Status);
        Assert.NotEqual(default, booking.CreatedAt);
        Assert.Null(booking.ProcessedAt);
    }

    [Fact]
    public void Create_ShouldThrowValidationException_WhenEventIdIsEmpty()
    {
        // Arrange
        var eventId = Guid.Empty;

        // Act
        var action = () => Booking.Create(eventId);

        // Assert
        Assert.Throws<ValidationException>(action);
    }

    [Fact]
    public void Confirm_ShouldSetConfirmedStatusAndProcessedAt()
    {
        // Arrange
        var booking = Booking.Create(Guid.NewGuid());

        // Act
        booking.Confirm();

        // Assert
        Assert.Equal(BookingStatus.Confirmed, booking.Status);
        Assert.NotNull(booking.ProcessedAt);
    }

    [Fact]
    public void Reject_ShouldSetRejectedStatusAndProcessedAt()
    {
        // Arrange
        var booking = Booking.Create(Guid.NewGuid());

        // Act
        booking.Reject();

        // Assert
        Assert.Equal(BookingStatus.Rejected, booking.Status);
        Assert.NotNull(booking.ProcessedAt);
    }

    [Fact]
    public void Confirm_ShouldThrowValidationException_WhenBookingAlreadyConfirmed()
    {
        // Arrange
        var booking = Booking.Create(Guid.NewGuid());
        booking.Confirm();

        var processedAt = booking.ProcessedAt;

        // Act
        var action = () => booking.Confirm();

        // Assert
        Assert.Throws<ValidationException>(action);
        Assert.Equal(BookingStatus.Confirmed, booking.Status);
        Assert.Equal(processedAt, booking.ProcessedAt);
    }

    [Fact]
    public void Reject_ShouldThrowValidationException_WhenBookingAlreadyConfirmed()
    {
        // Arrange
        var booking = Booking.Create(Guid.NewGuid());
        booking.Confirm();

        var processedAt = booking.ProcessedAt;

        // Act
        var action = () => booking.Reject();

        // Assert
        Assert.Throws<ValidationException>(action);
        Assert.Equal(BookingStatus.Confirmed, booking.Status);
        Assert.Equal(processedAt, booking.ProcessedAt);
    }

    [Fact]
    public void Confirm_ShouldThrowValidationException_WhenBookingAlreadyRejected()
    {
        // Arrange
        var booking = Booking.Create(Guid.NewGuid());
        booking.Reject();

        var processedAt = booking.ProcessedAt;

        // Act
        var action = () => booking.Confirm();

        // Assert
        Assert.Throws<ValidationException>(action);
        Assert.Equal(BookingStatus.Rejected, booking.Status);
        Assert.Equal(processedAt, booking.ProcessedAt);
    }
}
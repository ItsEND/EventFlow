using EventFlow.Bookings.Application.Abstractions.Messaging.Outbox;
using EventFlow.Bookings.Application.Abstractions.Repositories;
using EventFlow.Bookings.Application.Abstractions.Services;
using EventFlow.Bookings.Application.Exceptions;
using EventFlow.Bookings.Application.Services;
using EventFlow.Bookings.Domain.Models;
using EventFlow.Bookings.Infrastructure.DataAccess;
using EventFlow.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace EventFlow.Tests;

public class BookingServiceTests : IDisposable
{
    private readonly ServiceProvider _provider = TestServiceProviderFactory.Create();

    public void Dispose() => _provider.Dispose();

    [Fact]
    public async Task CreateBookingAsync_ShouldCreatePendingBooking()
    {
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await using var scope = _provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IBookingService>();

        var booking = await service.CreateBookingAsync(
            eventId,
            userId,
            TestContext.Current.CancellationToken);

        Assert.NotEqual(Guid.Empty, booking.Id);
        Assert.Equal(eventId, booking.EventId);
        Assert.Equal(userId, booking.UserId);
        Assert.Equal(BookingStatus.Pending.ToString(), booking.Status);
        Assert.Null(booking.ProcessedAt);
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldCreateUniqueIds()
    {
        await using var scope = _provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ct = TestContext.Current.CancellationToken;

        var first = await service.CreateBookingAsync(eventId, userId, ct);
        var second = await service.CreateBookingAsync(eventId, userId, ct);

        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public async Task GetBookingByIdAsync_ShouldReturnCreatedBooking()
    {
        await using var scope = _provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var ct = TestContext.Current.CancellationToken;
        var created = await service.CreateBookingAsync(Guid.NewGuid(), Guid.NewGuid(), ct);

        var found = await service.GetBookingByIdAsync(created.Id, ct);

        Assert.Equal(created, found);
    }

    [Fact]
    public async Task ProcessBookingAsync_ShouldConfirmPendingBooking()
    {
        await using var scope = _provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var ct = TestContext.Current.CancellationToken;
        var created = await service.CreateBookingAsync(Guid.NewGuid(), Guid.NewGuid(), ct);

        var processed = await service.ProcessBookingAsync(created.Id, ct);

        Assert.Equal(BookingStatus.Confirmed.ToString(), processed.Status);
        Assert.NotNull(processed.ProcessedAt);
    }
    [Fact]
    public async Task ProcessBookingAsync_ShouldNotConfirmBooking_WhenOutboxWritingFails()
    {
        var ct = TestContext.Current.CancellationToken;
        Guid bookingId;

        await using (var createScope = _provider.CreateAsyncScope())
        {
            var bookingService = createScope.ServiceProvider
                .GetRequiredService<IBookingService>();

            var booking = await bookingService.CreateBookingAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                ct);

            bookingId = booking.Id;
        }

        await using (var processScope = _provider.CreateAsyncScope())
        {
            var bookingRepository = processScope.ServiceProvider
                .GetRequiredService<IBookingRepository>();

            var bookingTaskQueue = processScope.ServiceProvider
                .GetRequiredService<IBookingTaskQueue>();

            var bookingService = new BookingService(
                bookingRepository,
                bookingTaskQueue,
                new FailingOutboxWriter());

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                bookingService.ProcessBookingAsync(bookingId, ct));
        }

        await using var verificationScope = _provider.CreateAsyncScope();

        var dbContext = verificationScope.ServiceProvider
            .GetRequiredService<BookingDbContext>();

        var savedBooking = await dbContext.Bookings
            .AsNoTracking()
            .SingleAsync(booking => booking.Id == bookingId, ct);

        Assert.Equal(BookingStatus.Pending, savedBooking.Status);

        Assert.False(await dbContext.OutboxMessages
            .AnyAsync(message => message.Id == bookingId, ct));
    }

    [Fact]
    public async Task ProcessBookingAsync_ShouldCreatePendingOutboxMessage()
    {
        await using var scope = _provider.CreateAsyncScope();

        var service = scope.ServiceProvider.GetRequiredService<IBookingService>();

        var dbContext = scope.ServiceProvider.GetRequiredService<BookingDbContext>();

        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ct = TestContext.Current.CancellationToken;

        var created = await service.CreateBookingAsync(eventId, userId, ct);

        var processed = await service.ProcessBookingAsync(created.Id, ct);

        var outboxMessage = await dbContext.OutboxMessages
            .SingleAsync(message => message.Id == created.Id, ct);

        Assert.Equal(KafkaTopics.BookingConfirmed, outboxMessage.Topic);

        Assert.Equal(eventId.ToString(), outboxMessage.MessageKey);

        Assert.Equal(processed.ProcessedAt, outboxMessage.OccurredAt);

        Assert.Null(outboxMessage.PublishedAt);
        Assert.Equal(0, outboxMessage.Attempts);
        Assert.Null(outboxMessage.LastError);

        var payload = JsonSerializer.Deserialize<BookingConfirmed>(outboxMessage.Payload);

        Assert.NotNull(payload);
        Assert.Equal(created.Id, payload.BookingId);
        Assert.Equal(eventId, payload.EventId);
        Assert.Equal(userId, payload.UserId);
        Assert.Equal(1, payload.SeatCount);
        Assert.Equal(processed.ProcessedAt, payload.ConfirmedAt);
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MissingBooking_ShouldProduceNotFoundError(bool process)
    {
        await using var scope = _provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var bookingId = Guid.NewGuid();
        var ct = TestContext.Current.CancellationToken;

        var exception = process
            ? await Assert.ThrowsAsync<AppException>(() => service.ProcessBookingAsync(bookingId, ct))
            : await Assert.ThrowsAsync<AppException>(() => service.GetBookingByIdAsync(bookingId, ct));

        Assert.Equal(AppErrorCode.NotFound, exception.Code);
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldEnforceLimitPerUser()
    {
        await using var scope = _provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var userId = Guid.NewGuid();
        var ct = TestContext.Current.CancellationToken;

        for (var index = 0; index < 10; index++)
        {
            await service.CreateBookingAsync(Guid.NewGuid(), userId, ct);
        }

        var exception = await Assert.ThrowsAsync<AppException>(() =>
            service.CreateBookingAsync(Guid.NewGuid(), userId, ct));

        Assert.Equal(AppErrorCode.BookingLimitExceeded, exception.Code);
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldUseSeparateLimitsForDifferentUsers()
    {
        await using var scope = _provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var firstUserId = Guid.NewGuid();
        var secondUserId = Guid.NewGuid();
        var ct = TestContext.Current.CancellationToken;

        for (var index = 0; index < 10; index++)
        {
            await service.CreateBookingAsync(Guid.NewGuid(), firstUserId, ct);
        }

        var secondUserBooking = await service.CreateBookingAsync(
            Guid.NewGuid(),
            secondUserId,
            ct);

        Assert.Equal(secondUserId, secondUserBooking.UserId);
    }

    [Fact]
    public async Task CancelBookingAsync_ShouldCancelOwnBooking()
    {
        await using var scope = _provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var userId = Guid.NewGuid();
        var ct = TestContext.Current.CancellationToken;
        var booking = await service.CreateBookingAsync(Guid.NewGuid(), userId, ct);

        await service.CancelBookingAsync(booking.Id, userId, isAdmin: false, ct);
        var cancelled = await service.GetBookingByIdAsync(booking.Id, ct);

        Assert.Equal(BookingStatus.Cancelled.ToString(), cancelled.Status);
        Assert.NotNull(cancelled.ProcessedAt);
    }

    [Fact]
    public async Task CancelBookingAsync_ShouldForbidAnotherUser()
    {
        await using var scope = _provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var ct = TestContext.Current.CancellationToken;
        var booking = await service.CreateBookingAsync(Guid.NewGuid(), Guid.NewGuid(), ct);

        var exception = await Assert.ThrowsAsync<AppException>(() =>
            service.CancelBookingAsync(booking.Id, Guid.NewGuid(), isAdmin: false, ct));

        Assert.Equal(AppErrorCode.Forbidden, exception.Code);
    }

    [Fact]
    public async Task CancelBookingAsync_ShouldAllowAdminToCancelAnyBooking()
    {
        await using var scope = _provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var ct = TestContext.Current.CancellationToken;
        var booking = await service.CreateBookingAsync(Guid.NewGuid(), Guid.NewGuid(), ct);

        await service.CancelBookingAsync(booking.Id, Guid.NewGuid(), isAdmin: true, ct);
        var cancelled = await service.GetBookingByIdAsync(booking.Id, ct);

        Assert.Equal(BookingStatus.Cancelled.ToString(), cancelled.Status);
    }

    [Fact]
    public void Cancel_ShouldThrowValidationException_WhenBookingAlreadyCancelled()
    {
        var booking = Booking.Create(Guid.NewGuid(), Guid.NewGuid());
        booking.Cancel();

        Assert.Throws<ValidationException>(booking.Cancel);
    }
    private sealed class FailingOutboxWriter : IOutboxWriter
    {
        public void Add(BookingConfirmed message)
        {
            throw new InvalidOperationException(
                "Имитация ошибки при записи в Outbox.");
        }
    }
}

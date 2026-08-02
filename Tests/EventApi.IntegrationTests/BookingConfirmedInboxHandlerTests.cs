using EventApi.IntegrationTests.Infrastructure;
using EventFlow.Contracts;
using EventFlow.Events.Domain.Models;
using EventFlow.Events.Infrastructure.Messaging.Inbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace EventApi.IntegrationTests;

[Collection(TestCollections.PostgreSql)]
public sealed class BookingConfirmedInboxHandlerTests
    : RepositoryTestBase
{
    public BookingConfirmedInboxHandlerTests(PostgreSqlFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task HandleAsync_WhenMessageDeliveredTwice_ShouldReserveSeatsOnce()
    {
        var ct = TestContext.Current.CancellationToken;

        var ev = Event.Create(
            title: "Inbox test event",
            description: null,
            totalSeats: 5,
            startAt: Utc(2030, 1, 1, 10, 0),
            endAt: Utc(2030, 1, 1, 12, 0));

        await using (var seedContext = CreateEventsContext())
        {
            seedContext.Events.Add(ev);
            await seedContext.SaveChangesAsync(ct);
        }

        var message = new BookingConfirmed(
            BookingId: Guid.NewGuid(),
            EventId: ev.Id,
            UserId: Guid.NewGuid(),
            SeatCount: 1,
            ConfirmedAt: DateTime.UtcNow);

        await using (var firstContext = CreateEventsContext())
        {
            var handler = new BookingConfirmedInboxHandler(firstContext,
                NullLogger<BookingConfirmedInboxHandler>.Instance);

            await handler.HandleAsync(message, ct);
        }

        await using (var secondContext = CreateEventsContext())
        {
            var handler = new BookingConfirmedInboxHandler(secondContext,
                NullLogger<BookingConfirmedInboxHandler>.Instance);

            await handler.HandleAsync(message, ct);
        }

        await using var verifyContext = CreateEventsContext();

        var savedEvent = await verifyContext.Events.AsNoTracking()
            .SingleAsync(currentEvent => currentEvent.Id == ev.Id,
                ct);

        var inboxMessage = await verifyContext.InboxMessages.AsNoTracking()
            .SingleAsync(inbox => inbox.MessageId == message.BookingId,
                ct);

        Assert.Equal(4, savedEvent.AvailableSeats);

        Assert.Equal(InboxMessageStatus.Processed, inboxMessage.Status);

        Assert.NotNull(inboxMessage.ProcessedAt);

        Assert.Single(await verifyContext.InboxMessages
                .AsNoTracking()
                .ToListAsync(ct));
    }
    [Fact]
    public async Task HandleAsync_WhenEventDoesNotExist_ShouldMarkMessageIgnored()
    {
        var ct = TestContext.Current.CancellationToken;
        var eventId = Guid.NewGuid();

        var message = new BookingConfirmed(
            BookingId: Guid.NewGuid(),
            EventId: eventId,
            UserId: Guid.NewGuid(),
            SeatCount: 1,
            ConfirmedAt: DateTime.UtcNow);

        await using (var context = CreateEventsContext())
        {
            var handler = new BookingConfirmedInboxHandler(
                context,
                NullLogger<BookingConfirmedInboxHandler>.Instance);

            await handler.HandleAsync(message, ct);
        }

        await using var verifyContext = CreateEventsContext();

        var inboxMessage = await verifyContext.InboxMessages
            .AsNoTracking()
            .SingleAsync(
                inbox => inbox.MessageId == message.BookingId,
                ct);

        Assert.Equal(
            InboxMessageStatus.IgnoredEventNotFound,
            inboxMessage.Status);

        Assert.NotNull(inboxMessage.ProcessedAt);
        Assert.NotNull(inboxMessage.Details);
        Assert.Contains(eventId.ToString(), inboxMessage.Details);
    }

    [Fact]
    public async Task HandleAsync_WhenSeatsAreNotAvailable_ShouldMarkMessageIgnored()
    {
        var ct = TestContext.Current.CancellationToken;

        var ev = Event.Create(
            title: "Full event",
            description: null,
            totalSeats: 1,
            startAt: Utc(2030, 2, 1, 10, 0),
            endAt: Utc(2030, 2, 1, 12, 0));

        await using (var seedContext = CreateEventsContext())
        {
            seedContext.Events.Add(ev);
            await seedContext.SaveChangesAsync(ct);
        }

        var message = new BookingConfirmed(
            BookingId: Guid.NewGuid(),
            EventId: ev.Id,
            UserId: Guid.NewGuid(),
            SeatCount: 2,
            ConfirmedAt: DateTime.UtcNow);

        await using (var context = CreateEventsContext())
        {
            var handler = new BookingConfirmedInboxHandler(
                context,
                NullLogger<BookingConfirmedInboxHandler>.Instance);

            await handler.HandleAsync(message, ct);
        }

        await using var verifyContext = CreateEventsContext();

        var savedEvent = await verifyContext.Events
            .AsNoTracking()
            .SingleAsync(
                currentEvent => currentEvent.Id == ev.Id,
                ct);

        var inboxMessage = await verifyContext.InboxMessages
            .AsNoTracking()
            .SingleAsync(
                inbox => inbox.MessageId == message.BookingId,
                ct);

        Assert.Equal(1, savedEvent.AvailableSeats);

        Assert.Equal(
            InboxMessageStatus.IgnoredNotEnoughSeats,
            inboxMessage.Status);

        Assert.NotNull(inboxMessage.ProcessedAt);
    }
    private static DateTime Utc(int year, int month, int day, int hour, int minute)
    {
        return new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Utc);
    }
}
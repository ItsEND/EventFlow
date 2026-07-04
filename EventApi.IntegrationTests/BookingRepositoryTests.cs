using EventApi.IntegrationTests.Infrastructure;
using EventFlow.Domain.Models;
using EventFlow.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EventApi.IntegrationTests;

[Collection(TestCollections.PostgreSql)]
public sealed class BookingRepositoryTests : RepositoryTestBase
{
    public BookingRepositoryTests(PostgreSqlFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task AddAsync_AndSaveChangesAsync_ShouldPersistBooking()
    {
        var existingEvent = await SeedEventAsync();
        var booking = Booking.Create(existingEvent.Id);

        await using (var context = CreateContext())
        {
            var repository = new BookingRepository(context);
            repository.Add(booking);
            await repository.SaveChangesAsync(CancellationToken.None);
        }

        await using var verifyContext = CreateContext();

        var saved = await verifyContext.Bookings.AsNoTracking()
            .SingleOrDefaultAsync(b => b.Id == booking.Id, CancellationToken.None);

        Assert.NotNull(saved);
        Assert.Equal(booking.Id, saved.Id);
        Assert.Equal(existingEvent.Id, saved.EventId);
        Assert.Equal(BookingStatus.Pending, saved.Status);
        Assert.Null(saved.ProcessedAt);
    }

    [Fact]
    public async Task GetPendingIdsAsync_ShouldReturnOnlyPendingBookings()
    {
        var existingEvent = await SeedEventAsync();

        var pendingBooking = Booking.Create(existingEvent.Id);
        var confirmedBooking = Booking.Create(existingEvent.Id);
        confirmedBooking.Confirm();

        await SeedBookingAsync(pendingBooking);
        await SeedBookingAsync(confirmedBooking);

        await using var context = CreateContext();
        var repository = new BookingRepository(context);

        var result = await repository.GetPendingIdsAsync(CancellationToken.None);

        Assert.Single(result);
        Assert.Contains(pendingBooking.Id, result);
        Assert.DoesNotContain(confirmedBooking.Id, result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnBooking_WhenBookingExists()
    {
        var existingEvent = await SeedEventAsync();
        var booking = Booking.Create(existingEvent.Id);

        await SeedBookingAsync(booking);

        await using var context = CreateContext();
        var repository = new BookingRepository(context);

        var result = await repository.GetByIdAsync(booking.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(booking.Id, result.Id);
        Assert.Equal(existingEvent.Id, result.EventId);
        Assert.Equal(BookingStatus.Pending, result.Status);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenBookingDoesNotExist()
    {
        await using var context = CreateContext();
        var repository = new BookingRepository(context);

        var result = await repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistBookingStatusChange()
    {
        var existingEvent = await SeedEventAsync();
        var booking = Booking.Create(existingEvent.Id);

        await SeedBookingAsync(booking);

        await using (var context = CreateContext())
        {
            var repository = new BookingRepository(context);

            var loadedBooking = await repository.GetByIdAsync(booking.Id, CancellationToken.None);
            Assert.NotNull(loadedBooking);

            loadedBooking.Confirm();
            await repository.SaveChangesAsync(CancellationToken.None);
        }

        await using var verifyContext = CreateContext();

        var saved = await verifyContext.Bookings
            .AsNoTracking()
            .SingleAsync(b => b.Id == booking.Id, CancellationToken.None);

        Assert.Equal(BookingStatus.Confirmed, saved.Status);
        Assert.NotNull(saved.ProcessedAt);
    }

    private async Task<Event> SeedEventAsync()
    {
        var existingEvent = Event.Create("Мероприятие для бронирования", null, 10, Utc(2026, 6, 1, 10), Utc(2026, 6, 1, 12));

        await using var context = CreateContext();

        context.Events.Add(existingEvent);

        await context.SaveChangesAsync(CancellationToken.None);

        return existingEvent;
    }

    private async Task SeedBookingAsync(Booking booking)
    {
        await using var context = CreateContext();

        context.Bookings.Add(booking);

        await context.SaveChangesAsync();
    }

    private static DateTime Utc(int year, int month, int day, int hour = 0, int minute = 0, int second = 0)
    {
        return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
    }
}

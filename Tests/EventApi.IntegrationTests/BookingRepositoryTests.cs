using EventApi.IntegrationTests.Infrastructure;
using EventFlow.Bookings.Domain.Models;
using EventFlow.Bookings.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EventApi.IntegrationTests;

[Collection(TestCollections.PostgreSql)]
public sealed class BookingRepositoryTests(PostgreSqlFixture fixture) : RepositoryTestBase(fixture)
{
    [Fact]
    public async Task Add_AndSaveChangesAsync_ShouldPersistBookingIdentifiers()
    {
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var booking = Booking.Create(eventId, userId);

        await using (var context = CreateBookingsContext())
        {
            var repository = new BookingRepository(context);
            repository.Add(booking);
            await repository.SaveChangesAsync(CancellationToken.None);
        }

        await using var verificationContext = CreateBookingsContext();
        var saved = await verificationContext.Bookings.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == booking.Id, CancellationToken.None);

        Assert.NotNull(saved);
        Assert.Equal(eventId, saved.EventId);
        Assert.Equal(userId, saved.UserId);
        Assert.Equal(BookingStatus.Pending, saved.Status);
    }

    [Fact]
    public async Task GetPendingIdsAsync_ShouldReturnOnlyPendingBookings()
    {
        var pending = Booking.Create(Guid.NewGuid(), Guid.NewGuid());
        var confirmed = Booking.Create(Guid.NewGuid(), Guid.NewGuid());
        confirmed.Confirm();
        await SeedBookingsAsync(pending, confirmed);

        await using var context = CreateBookingsContext();
        var repository = new BookingRepository(context);

        var result = await repository.GetPendingIdsAsync(CancellationToken.None);

        Assert.Equal([pending.Id], result);
    }

    [Fact]
    public async Task CountActiveByUserIdAsync_ShouldCountPendingAndConfirmedBookings()
    {
        var userId = Guid.NewGuid();
        var pending = Booking.Create(Guid.NewGuid(), userId);
        var confirmed = Booking.Create(Guid.NewGuid(), userId);
        confirmed.Confirm();
        var cancelled = Booking.Create(Guid.NewGuid(), userId);
        cancelled.Cancel();
        await SeedBookingsAsync(pending, confirmed, cancelled);

        await using var context = CreateBookingsContext();
        var repository = new BookingRepository(context);

        var result = await repository.CountActiveByUserIdAsync(userId, CancellationToken.None);

        Assert.Equal(2, result);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistBookingStatusChange()
    {
        var booking = Booking.Create(Guid.NewGuid(), Guid.NewGuid());
        await SeedBookingsAsync(booking);

        await using (var context = CreateBookingsContext())
        {
            var repository = new BookingRepository(context);
            var loaded = await repository.GetByIdAsync(booking.Id, CancellationToken.None);
            Assert.NotNull(loaded);
            loaded.Confirm();
            await repository.SaveChangesAsync(CancellationToken.None);
        }

        await using var verificationContext = CreateBookingsContext();
        var saved = await verificationContext.Bookings.AsNoTracking()
            .SingleAsync(item => item.Id == booking.Id, CancellationToken.None);

        Assert.Equal(BookingStatus.Confirmed, saved.Status);
        Assert.NotNull(saved.ProcessedAt);
    }

    private async Task SeedBookingsAsync(params Booking[] bookings)
    {
        await using var context = CreateBookingsContext();
        context.Bookings.AddRange(bookings);
        await context.SaveChangesAsync(CancellationToken.None);
    }
}

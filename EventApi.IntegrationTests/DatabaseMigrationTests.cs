using EventApi.IntegrationTests.Infrastructure;
using EventFlow.Domain.Models;
using EventFlow.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace EventApi.IntegrationTests;

[Collection(TestCollections.PostgreSql)]
public class DatabaseMigrationTests : RepositoryTestBase
{
    public DatabaseMigrationTests(PostgreSqlFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Migrations_ShouldCreateEventsAndBookingsTables()
    {
        await using var context = CreateContext();

        var eventsTableExists = await TableExistsAsync(context, "event");
        var bookingsTableExists = await TableExistsAsync(context, "booking");

        Assert.True(eventsTableExists);
        Assert.True(bookingsTableExists);
    }

    [Fact]
    public async Task Migrations_ShouldCreateForeignKeyBetweenBookingsAndEvents()
    {
        await using var context = CreateContext();

        var booking = Booking.Create(Guid.NewGuid());

        context.Bookings.Add(booking);

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            context.SaveChangesAsync(CancellationToken.None));
    }

    [Fact]
    public async Task Migrations_ShouldCascadeDeleteBookings_WhenEventIsDeleted()
    {
        var existingEvent = Event.Create("Cascade delete test", description: null, totalSeats: 10, startAt: Utc(2026, 6, 1, 10), endAt: Utc(2026, 6, 1, 12));
        var booking = Booking.Create(existingEvent.Id);

        await using (var context = CreateContext())
        {
            context.Events.Add(existingEvent);
            context.Bookings.Add(booking);

            await context.SaveChangesAsync(CancellationToken.None);
        }

        await using (var context = CreateContext())
        {
            var eventToDelete = await context.Events.SingleAsync(e => e.Id == existingEvent.Id, CancellationToken.None);

            context.Events.Remove(eventToDelete);

            await context.SaveChangesAsync(CancellationToken.None);
        }

        await using (var verifyContext = CreateContext())
        {
            var bookingExists = await verifyContext.Bookings.AnyAsync(b => b.Id == booking.Id, CancellationToken.None);

            Assert.False(bookingExists);
        }
    }

    private static async Task<bool> TableExistsAsync(AppDbContext context, string tableName)
    {
        await context.Database.OpenConnectionAsync(CancellationToken.None);

        try
        {
            await using var command = context.Database.GetDbConnection().CreateCommand();

            command.CommandText = """
                SELECT EXISTS (SELECT 1
                    FROM information_schema.tables
                    WHERE table_schema = 'public'
                      AND table_name = @tableName
                );
                """;

            var parameter = command.CreateParameter();
            parameter.ParameterName = "tableName";
            parameter.Value = tableName;

            command.Parameters.Add(parameter);

            var result = await command.ExecuteScalarAsync(CancellationToken.None);

            return result is bool exists && exists;
        }
        finally
        {
            await context.Database.CloseConnectionAsync();
        }
    }

    private static DateTime Utc(int year, int month, int day, int hour = 0, int minute = 0, int second = 0)
    {
        return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
    }
}

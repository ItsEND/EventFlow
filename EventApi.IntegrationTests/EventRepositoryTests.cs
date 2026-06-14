using EventApi.IntegrationTests.Infrastructure;
using EventFlow.Api.Models;
using EventFlow.Api.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit.Sdk;

namespace EventApi.IntegrationTests;

[Collection(TestCollections.PostgreSql)]
public class EventRepositoryTests : RepositoryTestBase
{
    public EventRepositoryTests(PostgreSqlFixture fixture) : base(fixture)
    {
    }
    [Fact]
    public async Task AddAsync_AndSaveChangesAsync_ShouldPersistEvent()
    {
        // Arrange
        var newEvent = CreateEvent("Новый митап", Utc(2026, 6, 1, 18), Utc(2026, 6, 1, 20));

        await using (var context = CreateContext())
        {
            var repository = new EventRepository(context);

            // Act
            repository.Add(newEvent);

            await repository.SaveChangesAsync(CancellationToken.None);
        }

        // Assert
        await using var verifyContext = CreateContext();

        var saved = await verifyContext.Events.AsNoTracking()
            .SingleOrDefaultAsync(e => e.Id == newEvent.Id, CancellationToken.None);

        Assert.NotNull(saved);
        Assert.Equal(newEvent.Id, saved.Id);
        Assert.Equal("Новый митап", saved.Title);
        Assert.Equal(10, saved.TotalSeats);
        Assert.Equal(10, saved.AvailableSeats);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEvent_WhenEventExists()
    {
        // Arrange
        var existingEvent = CreateEvent("Backend Meetup", Utc(2026, 4, 15, 10), Utc(2026, 4, 15, 18));

        await SeedEventsAsync(existingEvent);

        await using var context = CreateContext();
        var repository = new EventRepository(context);

        // Act
        var result = await repository.GetByIdAsync(existingEvent.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existingEvent.Id, result.Id);
        Assert.Equal(existingEvent.Title, result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenEventDoesNotExist()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new EventRepository(context);

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RemoveEventAsync_AndSaveChangesAsync_ShouldDeleteEvent()
    {
        // Arrange
        var existingEvent = CreateEvent("Удаляемое мероприятие", Utc(2026, 6, 1, 10), Utc(2026, 6, 1, 12));

        await SeedEventsAsync(existingEvent);

        await using (var context = CreateContext())
        {
            var repository = new EventRepository(context);

            var loadedEvent = await repository.GetByIdAsync(existingEvent.Id, CancellationToken.None);

            Assert.NotNull(loadedEvent);

            // Act
            repository.Remove(loadedEvent, CancellationToken.None);

            await repository.SaveChangesAsync(CancellationToken.None);
        }

        // Assert
        await using var verifyContext = CreateContext();

        var exists = await verifyContext.Events.AnyAsync(e => e.Id == existingEvent.Id, CancellationToken.None);

        Assert.False(exists);
    }

    [Theory]
    [MemberData(nameof(FilterCases))]
    public async Task GetPageAsync_ShouldApplyFilters(string? title, DateTime? dateFrom, DateTime? dateTo, string[] expectedTitles)
    {
        // Arrange
        await SeedEventsAsync(CreateSeedEvents());

        await using var context = CreateContext();
        var repository = new EventRepository(context);

        // Act
        var result = await repository.GetPageAsync(title, dateFrom, dateTo, 1, 50, CancellationToken.None);

        // Assert
        Assert.Equal(expectedTitles.Length, result.TotalItems);

        Assert.Equal(expectedTitles, result.Items.Select(e => e.Title).ToArray());
    }

    [Fact]
    public async Task GetPageAsync_ShouldApplyPaging()
    {
        // Arrange
        await SeedEventsAsync(CreateSeedEvents());

        await using var context = CreateContext();
        var repository = new EventRepository(context);

        // Act
        var result = await repository.GetPageAsync(null, null, null, 2, 2, CancellationToken.None);

        // Assert
        Assert.Equal(5, result.TotalItems);

        Assert.Equal(
            [
                "Docker Meetup",
                "ASP.NET Workshop"
            ],
            [.. result.Items.Select(e => e.Title)]);
    }

    public static TheoryData<string?, DateTime?, DateTime?, string[]> FilterCases
    {
        get
        {
            return new TheoryData<string?, DateTime?, DateTime?, string[]>
            {
                {null, null, null,
                    [
                        "Final Meetup",
                        "REST Conference",
                        "Docker Meetup",
                        "ASP.NET Workshop",
                        "Backend Meetup"
                    ]
                },
                { "MEETUP", null, null,
                    [
                        "Final Meetup",
                        "Docker Meetup",
                        "Backend Meetup"
                    ]
                },
                { "   ", null, null,
                    [
                        "Final Meetup",
                        "REST Conference",
                        "Docker Meetup",
                        "ASP.NET Workshop",
                        "Backend Meetup"
                    ]
                },
                {
                    null, Utc(2026, 5, 1), null,
                    [

                        "Final Meetup",
                        "REST Conference",
                        "Docker Meetup"
                    ]
                },

                {
                    null, null, Utc(2026, 5, 10, 23, 59, 59),
                    [
                        "REST Conference",
                        "Docker Meetup",
                        "ASP.NET Workshop",
                        "Backend Meetup"
                    ]
                },

                {
                    null, Utc(2026, 5, 1), Utc(2026, 5, 15, 23, 59, 59), ["REST Conference", "Docker Meetup"]
                },

                {
                    "MEETUP", Utc(2026, 5, 1), Utc(2026, 5, 15, 23, 59, 59), ["Docker Meetup"]
                }
            };
        }
    }

    private async Task SeedEventsAsync(params Event[] events)
    {
        await using var context = CreateContext();

        context.Events.AddRange(events);

        await context.SaveChangesAsync();
    }

    private static Event[] CreateSeedEvents()
    {
        return
        [
            CreateEvent("Backend Meetup",Utc(2026, 4, 15, 10),Utc(2026, 4, 15, 18)),

            CreateEvent("ASP.NET Workshop",Utc(2026, 4, 18, 11),Utc(2026, 4, 18, 14)),

            CreateEvent("Docker Meetup",Utc(2026, 5, 2, 12),Utc(2026, 5, 2, 15)),

            CreateEvent("REST Conference",Utc(2026, 5, 10, 10),Utc(2026, 5, 10, 18)),

            CreateEvent("Final Meetup",Utc(2026, 5, 20, 19),Utc(2026, 5, 20, 21))
        ];
    }

    private static Event CreateEvent(string title, DateTime startAt, DateTime endAt)
    {
        return Event.Create(title, description: null, totalSeats: 10, startAt, endAt);
    }

    private static DateTime Utc(int year, int month, int day, int hour = 0, int minute = 0, int second = 0)
    {
        return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
    }
}
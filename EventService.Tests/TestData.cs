using EventFlow.Api.DataAccess;
using EventFlow.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventService.Tests;

internal static class TestData
{
    /// <summary>
    /// Заполняет тестовую базу стандартным набором мероприятий.
    /// </summary>
    public static List<Event> SeedEvents(IServiceProvider provider)
    {
        var events = CreateEvents();

        using var scope = provider.CreateScope();

        var context =
            scope.ServiceProvider.GetRequiredService<AppDbContext>();

        context.Events.AddRange(events);
        context.SaveChanges();

        return events;
    }

    /// <summary>
    /// Добавляет одно мероприятие в тестовую базу.
    /// </summary>
    public static void AddEvent(IServiceProvider provider, Event ev)
    {
        using var scope = provider.CreateScope();

        var context =
            scope.ServiceProvider.GetRequiredService<AppDbContext>();

        context.Events.Add(ev);
        context.SaveChanges();
    }

    /// <summary>
    /// Возвращает актуальное количество свободных мест из базы.
    /// </summary>
    public static async Task<int> GetAvailableSeatsAsync(
        IServiceProvider provider,
        Guid eventId)
    {
        await using var scope = provider.CreateAsyncScope();

        var context =
            scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await context.Events
            .AsNoTracking()
            .Where(ev => ev.Id == eventId)
            .Select(ev => ev.AvailableSeats)
            .SingleAsync();
    }

    /// <summary>
    /// Возвращает количество броней в базе.
    /// </summary>
    public static async Task<int> GetBookingCountAsync(
        IServiceProvider provider)
    {
        await using var scope = provider.CreateAsyncScope();

        var context =
            scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await context.Bookings.CountAsync();
    }

    private static List<Event> CreateEvents()
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
                15,
                new DateTime(2026, 4, 16, 18, 30, 0),
                new DateTime(2026, 4, 16, 20, 30, 0)),

            Event.Create(
                "Воркшоп по LINQ",
                "Фильтрация, группировка и проекции",
                10,
                new DateTime(2026, 4, 18, 11, 0, 0),
                new DateTime(2026, 4, 18, 14, 0, 0)),

            Event.Create(
                "Архитектура REST API",
                "Проектирование контроллеров, DTO и сервисов",
                41,
                new DateTime(2026, 4, 20, 9, 30, 0),
                new DateTime(2026, 4, 20, 12, 30, 0)),

            Event.Create(
                "Конференция по тестированию",
                "Unit-тесты, интеграционные тесты и моки",
                10,
                new DateTime(2026, 4, 22, 10, 0, 0),
                new DateTime(2026, 4, 22, 17, 0, 0)),

            Event.Create(
                "SQL Meetup",
                "Практика написания SQL-запросов",
                5,
                new DateTime(2026, 4, 25, 17, 0, 0),
                new DateTime(2026, 4, 25, 19, 0, 0)),

            Event.Create(
                "Docker для разработчиков",
                "Контейнеризация приложений .NET",
                4,
                new DateTime(2026, 5, 2, 12, 0, 0),
                new DateTime(2026, 5, 2, 15, 0, 0)),

            Event.Create(
                "Митап по ASP.NET Core",
                "Middleware, маршрутизация и обработка ошибок",
                999,
                new DateTime(2026, 5, 5, 18, 0, 0),
                new DateTime(2026, 5, 5, 20, 0, 0)),

            Event.Create(
                "Воркшоп Swagger/OpenAPI",
                "Документирование и тестирование API",
                1,
                new DateTime(2026, 5, 8, 14, 0, 0),
                new DateTime(2026, 5, 8, 16, 30, 0)),

            Event.Create(
                "Конференция Clean Code",
                "Чистый код, читаемость и поддерживаемость",
                10,
                new DateTime(2026, 5, 12, 10, 0, 0),
                new DateTime(2026, 5, 12, 18, 0, 0)),

            Event.Create(
                "Практика по пагинации",
                "Skip, Take и работа с коллекциями",
                5,
                new DateTime(2026, 5, 15, 13, 0, 0),
                new DateTime(2026, 5, 15, 15, 0, 0)),

            Event.Create(
                "Финальный митап спринта",
                "Подведение итогов и разбор проекта",
                6,
                new DateTime(2026, 5, 20, 19, 0, 0),
                new DateTime(2026, 5, 20, 21, 0, 0))
        ];
    }
}
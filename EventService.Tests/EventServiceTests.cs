using EventFlow.Api.Contracts;
using EventFlow.Api.Models;

namespace EventService.Tests
{
    public class EventServiceTests
    {
        private readonly EventFlow.Api.Services.EventService _eventService;

        public EventServiceTests()
        {
            _eventService = new EventFlow.Api.Services.EventService(SeedEvents());
        }

        [Fact]
        public void FilterByTitle_ReturnsMatchingEvents()
        {
            //Arrange
            var searchSubstring = ".NET";
            var expectedResult = new List<string> { "Конференция .NET Backend", "Митап по ASP.NET Core" };
            var notExpectedResult = new List<string> { "Митап C# Junior", "Воркшоп по LINQ", "Архитектура REST API" };
            var pageData = new GetEventsQuery { Title = searchSubstring };

            //Act
            var result = _eventService.GetEvents(pageData).Items;

            //Assert
            Assert.All(result, ev => expectedResult.Contains(ev.Title));
            Assert.DoesNotContain(result.Select(ev => ev.Title), title => notExpectedResult.Contains(title));
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
                new DateTime(2026, 4, 16, 20, 30, 0)),

            Event.Create(
                "Воркшоп по LINQ",
                "Фильтрация, группировка и проекции",
                new DateTime(2026, 4, 18, 11, 0, 0),
                new DateTime(2026, 4, 18, 14, 0, 0)),

            Event.Create(
                "Архитектура REST API",
                "Проектирование контроллеров, DTO и сервисов",
                new DateTime(2026, 4, 20, 9, 30, 0),
                new DateTime(2026, 4, 20, 12, 30, 0)),

            Event.Create(
                "Конференция по тестированию",
                "Unit-тесты, интеграционные тесты и моки",
                new DateTime(2026, 4, 22, 10, 0, 0),
                new DateTime(2026, 4, 22, 17, 0, 0)),

            Event.Create(
                "SQL Meetup",
                "Практика написания SQL-запросов",
                new DateTime(2026, 4, 25, 17, 0, 0),
                new DateTime(2026, 4, 25, 19, 0, 0)),

            Event.Create(
                "Docker для разработчиков",
                "Контейнеризация приложений .NET",
                new DateTime(2026, 5, 2, 12, 0, 0),
                new DateTime(2026, 5, 2, 15, 0, 0)),

            Event.Create(
                "Митап по ASP.NET Core",
                "Middleware, маршрутизация и обработка ошибок",
                new DateTime(2026, 5, 5, 18, 0, 0),
                new DateTime(2026, 5, 5, 20, 0, 0)),

            Event.Create(
                "Воркшоп Swagger/OpenAPI",
                "Документирование и тестирование API",
                new DateTime(2026, 5, 8, 14, 0, 0),
                new DateTime(2026, 5, 8, 16, 30, 0)),

            Event.Create(
                "Конференция Clean Code",
                "Чистый код, читаемость и поддерживаемость",
                new DateTime(2026, 5, 12, 10, 0, 0),
                new DateTime(2026, 5, 12, 18, 0, 0)),

            Event.Create(
                "Практика по пагинации",
                "Skip, Take и работа с коллекциями",
                new DateTime(2026, 5, 15, 13, 0, 0),
                new DateTime(2026, 5, 15, 15, 0, 0)),

            Event.Create(
                "Финальный митап спринта",
                "Подведение итогов и разбор проекта",
                new DateTime(2026, 5, 20, 19, 0, 0),
                new DateTime(2026, 5, 20, 21, 0, 0))
            ];
        }


    }
}

using EventFlow.Api;
using EventFlow.Api.Contracts;
using EventFlow.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace EventService.Tests
{
    public class EventServiceTests
    {
        private readonly EventFlow.Api.Services.EventService _eventService;
        private readonly List<Event> _seedEvents;

        public EventServiceTests()
        {
            _seedEvents = SeedEvents();
            _eventService = new EventFlow.Api.Services.EventService(_seedEvents);

        }

        [Fact]
        public void AddEvent_ShouldCreateEvent()
        {
            // Arrange
            var newEvent = new CreateEventModel
            {
                Title = "Новый Митап",
                Description = "Описание нового митапа",
                StartAt = new DateTime(2026, 6, 1, 18, 0, 0),
                EndAt = new DateTime(2026, 6, 1, 20, 0, 0)
            };

            var beforeCount = _eventService.GetEvents(new GetEventsQuery()).TotalItems;

            // Act
            var created = _eventService.AddEvent(newEvent);

            var after = _eventService.GetEvent(created.Id);
            var afterCount = _eventService.GetEvents(new GetEventsQuery()).TotalItems;

            // Assert
            Assert.NotNull(created);
            Assert.Equal(beforeCount + 1, afterCount);

            Assert.Equal(newEvent.Title, after.Title);
            Assert.Equal(newEvent.Description, after.Description);
            Assert.Equal(newEvent.StartAt, after.StartAt);
            Assert.Equal(newEvent.EndAt, after.EndAt);
        }

        [Fact]
        public void GetEvents_ReturnsAllEvents()
        {
            // Arrange
            var expectedCount = _seedEvents.Count;

            var query = new GetEventsQuery
            {
                Page = 1,
                PageSize = 50
            };

            // Act
            var result = _eventService.GetEvents(query);

            // Assert
            Assert.Equal(expectedCount, result.TotalItems);
            Assert.Equal(expectedCount, result.Items.Count());
        }

        [Fact]
        public void GetEvent_ShouldReturnWhenIdExists()
        {
            //Arrange
            var existingEvent = _seedEvents.First();

            //Act
            var result = _eventService.GetEvent(existingEvent.Id);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(existingEvent.Id, result.Id);
            Assert.Equal(existingEvent.Title, result.Title);
            Assert.Equal(existingEvent.Description, result.Description);
            Assert.Equal(existingEvent.StartAt, result.StartAt);
            Assert.Equal(existingEvent.EndAt, result.EndAt);
        }

        [Fact]
        public void UpdateEvent_ShouldModifyExistingEvent()
        {
            //Arrange
            var existingEvent = _seedEvents.First();
            var updateModel = new UpdateEventModel
            {
                Title = "Обновленное название",
                Description = "Обновленное описание",
                StartAt = existingEvent.StartAt.AddHours(1),
                EndAt = existingEvent.EndAt.AddHours(1)
            };

            //Act
            var updated = _eventService.UpdateEvent(existingEvent.Id, updateModel);
            var afterUpdate = _eventService.GetEvent(existingEvent.Id);

            //Assert
            Assert.NotNull(updated);
            Assert.Equal(existingEvent.Id, updated.Id);
            Assert.Equal(updateModel.Title, afterUpdate.Title);
            Assert.Equal(updateModel.Description, afterUpdate.Description);
            Assert.Equal(updateModel.StartAt, afterUpdate.StartAt);
            Assert.Equal(updateModel.EndAt, afterUpdate.EndAt);
        }

        [Fact]
        public void RemoveEvent_ShouldDeleteExistingEvent()
        {
            //Arrange
            var existingEvent = _seedEvents.First();
            var beforeCount = _eventService.GetEvents(new GetEventsQuery()).TotalItems;

            //Act
            _eventService.RemoveEvent(existingEvent.Id);
            var afterCount = _eventService.GetEvents(new GetEventsQuery()).TotalItems;

            //Assert
            Assert.Equal(beforeCount - 1, afterCount);
            Assert.Throws<NotFoundException>(() => _eventService.GetEvent(existingEvent.Id));
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

        [Fact]
        public void GetEvents_ShouldFilterByDateRange()
        {
            //Arrange

            var query = new GetEventsQuery
            {
                From = new DateTime(2026, 5, 1),
                To = new DateTime(2026, 5, 10, 23, 59, 59)
            };

            //Act
            var result = _eventService.GetEvents(query);
            var titles = result.Items.Select(e => e.Title).ToList();

            //Assert
            Assert.Equal(3, result.TotalItems);
            Assert.Contains("Docker для разработчиков", titles);
            Assert.Contains("Митап по ASP.NET Core", titles);
            Assert.Contains("Воркшоп Swagger/OpenAPI", titles);
        }

        [Fact]
        public void GetEvents_ShouldReturnRequestedPage()
        {
            // Arrange
            var query = new GetEventsQuery
            {
                Page = 2,
                PageSize = 5
            };

            var expectedPageItems = _seedEvents
                .OrderByDescending(x => x.StartAt)
                .Skip(5)
                .Take(5)
                .ToList();

            // Act
            var result = _eventService.GetEvents(query);

            // Assert
            Assert.Equal(2, result.CurrentPage);
            Assert.Equal(5, result.PageSize);
            Assert.Equal(12, result.TotalItems);
            Assert.Equal(3, result.TotalPages);
            Assert.Equal(5, result.TotalItemsOnPage);
            Assert.Equal(expectedPageItems.Select(x => x.Id), result.Items.Select(x => x.Id));
        }

        [Fact]
        public void GetEvents_ShouldApplyCombinedFilters()
        {
            //Arrange
            var query = new GetEventsQuery
            {
                Title = "Митап",
                From = new DateTime(2026, 5, 1),
                To = new DateTime(2026, 5, 31, 23, 59, 59)
            };

            //Act
            var result = _eventService.GetEvents(query);
            var titles = result.Items.Select(e => e.Title).ToList();

            //Assert
            Assert.Equal(2, result.TotalItems);
            Assert.Contains("Митап по ASP.NET Core", titles);
            Assert.Contains("Финальный митап спринта", titles);
        }

        [Fact]
        public void GetEvent_ShouldThrowNotFoundException_WhenIdDoesNotExist()
        {
            //Arrange
            var nonExistentId = Guid.NewGuid();
            
            //Act
            var action = () => _eventService.GetEvent(nonExistentId);
            
            //Assert
            Assert.Throws<NotFoundException>(action);
        }
        [Fact]
        public void UpdateEvent_ShouldThrowNotFoundException_WhenIdDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();

            var updateModel = new UpdateEventModel
            {
                Title = "Обновление",
                Description = "Описание",
                StartAt = new DateTime(2026, 6, 1, 10, 0, 0),
                EndAt = new DateTime(2026, 6, 1, 12, 0, 0)
            };

            // Act
            var action = () => _eventService.UpdateEvent(id, updateModel);

            // Assert
            Assert.Throws<NotFoundException>(action);
        }

        [Fact]
        public void AddEvent_ShouldThrowValidationException_WhenTitleIsInvalid()
        {
            // Arrange
            var invalidEvent = new CreateEventModel
            {
                Title = "   ",
                Description = "Описание",
                StartAt = new DateTime(2026, 6, 1, 10, 0, 0),
                EndAt = new DateTime(2026, 6, 1, 12, 0, 0)
            };

            // Act
            var action = () => _eventService.AddEvent(invalidEvent);

            // Assert
            Assert.Throws<ValidationException>(action);
        }

        [Fact]
        public void UpdateEvent_ShouldThrowValidationException_WhenEndAtEarlierThanStartAt()
        {
            // Arrange
            var existingEvent = _seedEvents.First();

            var invalidUpdate = new UpdateEventModel
            {
                Title = "Некорректное обновление",
                Description = "Описание",
                StartAt = new DateTime(2026, 6, 10, 15, 0, 0),
                EndAt = new DateTime(2026, 6, 10, 10, 0, 0)
            };

            // Act
            var action = () => _eventService.UpdateEvent(existingEvent.Id, invalidUpdate);

            // Assert
            Assert.Throws<ValidationException>(action);
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

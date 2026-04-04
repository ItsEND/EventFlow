using EventFlow.Api.Contracts;
using EventFlow.Api.Models;
using EventFlow.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventFlow.Api.Controllers;

/// <summary>
/// Контроллер для управления мероприятиями.
/// </summary>
/// <param name="_eventService">Сервис для работы с мероприятиями.</param>
[ApiController]
[Route("events")]
public class EventController(IEventService _eventService) : ControllerBase
{
    /// <summary>
    /// Возвращает список всех мероприятий.
    /// </summary>
    /// <returns>Список мероприятий.</returns>
    [HttpGet]
    public ActionResult<List<EventResponse>> GetAllEvents()
    {
        var events = _eventService.GetEvents().Select(ToResponse).ToList();

        return Ok(events);
    }

    /// <summary>
    /// Возвращает мероприятие по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор мероприятия.</param>
    /// <returns>Найденное мероприятие.</returns>
    [HttpGet("{id:Guid}")]
    public ActionResult<EventResponse> GetEventById(Guid id)
    {
        var ev = _eventService.GetEvent(id);

        return ev is null ? (ActionResult<EventResponse>)NotFound() : (ActionResult<EventResponse>)Ok(ToResponse(ev));
    }

    /// <summary>
    /// Создает новое мероприятие.
    /// </summary>
    /// <param name="request">Данные для создания мероприятия.</param>
    /// <returns>Созданное мероприятие.</returns>
    [HttpPost]
    public ActionResult<EventResponse> CreateEvent([FromBody] EventRequest request)
    {
        var created = _eventService.AddEvent(new CreateEventModel
        {
            Title = request.Title,
            Description = request.Description,
            StartAt = request.StartAt,
            EndAt = request.EndAt
        });

        var response = ToResponse(created);

        return CreatedAtAction(nameof(GetEventById), new { id = response.Id }, response);
    }

    /// <summary>
    /// Обновляет мероприятие по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор мероприятия.</param>
    /// <param name="request">Новые данные мероприятия.</param>
    /// <returns>Обновленное мероприятие.</returns>
    [HttpPut("{id:Guid}")]
    public IActionResult UpdateEvent(Guid id, [FromBody] UpdateEventRequest request)
    {
        var updated = _eventService.UpdateEvent(id, new UpdateEventModel
        {
            Title = request.Title,
            Description = request.Description,
            StartAt = request.StartAt,
            EndAt = request.EndAt
        });

        return updated is null ? NotFound() : Ok(ToResponse(updated));
    }


    /// <summary>
    /// Удаляет мероприятие по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор мероприятия.</param>
    /// <returns>Результат удаления мероприятия.</returns>
    [HttpDelete("{id:Guid}")]
    public IActionResult Delete(Guid id)
    {
        return _eventService.RemoveEvent(id) ? NoContent() : NotFound();
    }

    private static EventResponse ToResponse(Event ev) => new()
    {
        Id = ev.Id,
        Title = ev.Title,
        Description = ev.Description,
        StartAt = ev.StartAt,
        EndAt = ev.EndAt
    };
}


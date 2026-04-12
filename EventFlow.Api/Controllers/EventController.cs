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
    /// 
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [HttpGet]
    public ActionResult<PaginatedResult<EventResponse>> GetPaginatedEvents([FromQuery] GetEventsQuery query)
    {
        var paginatedEvents = _eventService.GetEvents(query.Page, query.PageSize, query.Title, query.From, query.To);
        var result = PaginatedEventToResponse(paginatedEvents);

        return Ok(result);
    }

    /// <summary>
    /// Возвращает мероприятие по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор мероприятия.</param>
    /// <returns>Найденное мероприятие.</returns>
    [HttpGet("{id:guid}")]
    public ActionResult<EventResponse> GetEventById(Guid id)
    {
        var ev = _eventService.GetEvent(id);

        return (ActionResult<EventResponse>)Ok(ToResponse(ev));
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
    [HttpPut("{id:guid}")]
    public IActionResult UpdateEvent(Guid id, [FromBody] UpdateEventRequest request)
    {
        var updated = _eventService.UpdateEvent(id, new UpdateEventModel
        {
            Title = request.Title,
            Description = request.Description,
            StartAt = request.StartAt,
            EndAt = request.EndAt
        });

        return Ok(ToResponse(updated));
    }


    /// <summary>
    /// Удаляет мероприятие по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор мероприятия.</param>
    /// <returns>Результат удаления мероприятия.</returns>
    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        _eventService.RemoveEvent(id);
        return NoContent();
    }

    private static EventResponse ToResponse(Event ev) => new()
    {
        Id = ev.Id,
        Title = ev.Title,
        Description = ev.Description,
        StartAt = ev.StartAt,
        EndAt = ev.EndAt
    };

    private static PaginatedResult<EventResponse> PaginatedEventToResponse(PaginatedResult<Event> result)
        => new(result.Items.Select(ToResponse), result.CurrentPage, result.TotalPages, result.TotalItems);
       
}


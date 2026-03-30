using EventFlow.Api.Contracts;
using EventFlow.Api.Models;
using EventFlow.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventController(IEventService _eventService) : ControllerBase
{
    [HttpGet]
    public ActionResult<List<EventResponse>> GetAllEvents()
    {
        var events = _eventService.GetEvents();

        List<EventResponse> responses = [];
        if (events != null)
        {
            foreach (var ev in events)
            {
                var response = CreateEventResponse(ev);
                responses.Add(response);
            }
        }
        return Ok(responses);
    }

    [HttpGet("{id:Guid}")]
    public ActionResult<EventResponse> GetEvent(Guid id)
    {
        var ev = _eventService.GetEvent(id);

        if (ev == null)
        {
            return (ActionResult<EventResponse>)NotFound();
        }
        var response = CreateEventResponse(ev);

        return (ActionResult<EventResponse>)Ok(response);
    }

    //TODO: из контроллера передать в сервис модель,
    //её передать в сервис там создать guid
    //и в контроллер вернуть event и
    //в нем уже переделать его под dto response
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<EventResponse> Post([FromBody] EventRequest newEvent)
    {
        var ev = new Event
        {
            Id = Guid.NewGuid(),
            Title = newEvent.Title,
            Description = newEvent.Description,
            StartAt = newEvent.StartAt,
            EndAt = newEvent.EndAt,
        };
        _eventService.AddEvent(ev);

        var response = CreateEventResponse(ev);

        return CreatedAtAction(nameof(GetEvent), response);
    }

    [HttpPut("{id:Guid}")]
    public IActionResult Put(Guid id, [FromBody] UpdateEventRequest newEvent)
    {
        if (id != newEvent.Id)
        {
            return BadRequest();
        }
        var ev = CreateEvent(newEvent);

        return _eventService.ChangeEvent(id, ev) ? NoContent() : NotFound();
    }

    [HttpDelete("{id:Guid}")]
    public IActionResult Delete(Guid id)
    {
        return _eventService.RemoveEvent(id) ? NoContent() : NotFound();
    }

    private EventResponse CreateEventResponse(Event ev) => new()
    {
        Id = ev.Id,
        Title = ev.Title,
        Description = ev.Description,
        StartAt = ev.StartAt,
        EndAt = ev.EndAt,
    };

    private Event CreateEvent(UpdateEventRequest request) => new()
    {
        Id = request.Id,
        Title = request.Title,
        Description = request.Description,
        StartAt = request.StartAt,
        EndAt = request.EndAt,
    };
}
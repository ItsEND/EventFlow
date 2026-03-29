using EventFlow.Api.Models;
using EventFlow.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventController(IEventService _eventService, ILogger<EventController> _logger) : ControllerBase
{
    [HttpGet]
    public ActionResult<List<Event>> GetAllEvents()
    {
        var events = _eventService.GetEvents();

        return Ok(events);
    }

    [HttpGet("{id:Guid}")]
    public ActionResult<Event> GetEvent(Guid id)
    {
        var ev = _eventService.GetEvent(id);
        return ev == null ? (ActionResult<Event>)NotFound() : (ActionResult<Event>)Ok(ev);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<Event> Post([FromBody] Event newEvent)
    {
        _eventService.AddEvent(newEvent);

        return CreatedAtAction(nameof(GetEvent), newEvent);
    }

    [HttpPut("{id:Guid}")]
    public IActionResult Put(Guid id, [FromBody] Event newEvent)
    {
        if (id != newEvent.Id)
        {
            return BadRequest();
        }

        return _eventService.ChangeEvent(id, newEvent) ? NoContent() : NotFound();
    }

    [HttpDelete("{id:Guid}")]
    public IActionResult Delete(Guid id)
    {
        return _eventService.RemoveEvent(id) ? NoContent() : NotFound();
    }
}

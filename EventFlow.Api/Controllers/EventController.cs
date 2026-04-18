using EventFlow.Api.Contracts;
using EventFlow.Api.Models;
using EventFlow.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventFlow.Api.Controllers;

/// <summary>
/// Контроллер для управления мероприятиями.
/// Предоставляет методы для получения, создания, обновления и удаления событий.
/// </summary>
/// <param name="_eventService">Сервис для работы с мероприятиями.</param>
[ApiController]
[Route("events")]
public class EventController(IEventService _eventService) : ControllerBase
{
    /// <summary>
    /// Возвращает список мероприятий с учетом фильтрации и пагинации.
    /// </summary>
    /// <param name="query">
    /// Параметры запроса: фильтрация по названию, диапазону дат,
    /// а также номер страницы и размер страницы.
    /// </param>
    /// <returns>
    /// Постраничный список мероприятий в виде <see cref="PaginatedResult{T}"/>.
    /// </returns>
    [HttpGet]
    public ActionResult<PaginatedResult<EventResponse>> GetEvents([FromQuery] GetEventsQuery query)
    {
        var paginatedEvents = _eventService.GetEvents(query);
        var result = PaginatedEventToResponse(paginatedEvents);

        return Ok(result);
    }

    /// <summary>
    /// Возвращает мероприятие по его уникальному идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор мероприятия.</param>
    /// <returns>Созданное мероприятие.</returns>
    [HttpGet("{id:guid}")]
    public ActionResult<EventResponse> GetEventById(Guid id)
    {
        var ev = _eventService.GetEvent(id);
        return Ok(ToResponse(ev));
    }

    /// <summary>
    /// Создает новое мероприятие.
    /// </summary>
    /// <param name="request">Данные для создания мероприятия.</param>
    /// <returns>Мероприятие.</returns>
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
    /// Обновляет существующее мероприятие по его идентификатору.
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
    /// Удаляет мероприятие по его идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор мероприятия.</param>
    /// <returns>Пустой ответ со статусом 204 No Content.</returns>
    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        _eventService.RemoveEvent(id);
        return NoContent();
    }

    /// <summary>
    /// Преобразует внутреннюю модель мероприятия в DTO ответа.
    /// </summary>
    /// <param name="ev">Мероприятие доменной модели.</param>
    /// <returns>Объект ответа с данными мероприятия.</returns>
    private static EventResponse ToResponse(Event ev) => new()
    {
        Id = ev.Id,
        Title = ev.Title,
        Description = ev.Description,
        StartAt = ev.StartAt,
        EndAt = ev.EndAt
    };

    /// <summary>
    /// Преобразует постраничный результат мероприятий доменной модели
    /// в постраничный результат DTO ответа.
    /// </summary>
    /// <param name="result">Постраничный результат доменной модели.</param>
    /// <returns>Постраничный результат DTO ответа.</returns>
    private static PaginatedResult<EventResponse> PaginatedEventToResponse(PaginatedResult<Event> result)
        => new(result.Items.Select(ToResponse), result.CurrentPage, result.PageSize, result.TotalPages, result.TotalItems, result.TotalItemsOnPage);

}


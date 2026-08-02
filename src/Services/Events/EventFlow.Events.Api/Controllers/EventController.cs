using EventFlow.Events.Api.Contracts;
using EventFlow.Events.Api.Contracts.Events;
using EventFlow.Events.Application.Abstractions.Services;
using EventFlow.Events.Application.Contracts;
using EventFlow.Events.Application.Contracts.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace EventFlow.Events.Api.Controllers;

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
    public async Task<ActionResult<PaginatedResult<EventResponse>>> GetEvents([FromQuery] GetEventsQuery query)
    {
        var result = PaginatedEventToResponse(await _eventService.GetEventsAsync(query));
        return Ok(result);
    }

    /// <summary>
    /// Возвращает мероприятие по его уникальному идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор мероприятия.</param>
    /// <returns>Созданное мероприятие.</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EventResponse>> GetEventById(Guid id)
    {
        var ev = await _eventService.GetEventAsync(id);
        return Ok(DtoHelper.ToEventResponse(ev));
    }

    /// <summary>
    /// Создает новое мероприятие.
    /// </summary>
    /// <param name="request">Данные для создания мероприятия.</param>
    /// <returns>Мероприятие.</returns>
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [HttpPost]
    public async Task<ActionResult<EventResponse>> CreateEvent([FromBody] EventRequest request)
    {
        var created = await _eventService.CreateEventAsync(new CreateEventModel
        {
            Title = request.Title,
            Description = request.Description,
            TotalSeats = request.TotalSeats!.Value,
            StartAt = request.StartAt,
            EndAt = request.EndAt
        });

        var response = DtoHelper.ToEventResponse(created);

        return CreatedAtAction(nameof(GetEventById), new { id = response.Id }, response);
    }

    /// <summary>
    /// Обновляет существующее мероприятие по его идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор мероприятия.</param>
    /// <param name="request">Новые данные мероприятия.</param>
    /// <returns>Обновленное мероприятие.</returns>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] UpdateEventRequest request)
    {
        var updated = await _eventService.UpdateEventAsync(id, new UpdateEventModel
        {
            Title = request.Title,
            Description = request.Description,
            StartAt = request.StartAt,
            EndAt = request.EndAt
        });

        return Ok(DtoHelper.ToEventResponse(updated));
    }

    /// <summary>
    /// Удаляет мероприятие по его идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор мероприятия.</param>
    /// <returns>Пустой ответ со статусом 204 No Content.</returns>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _eventService.RemoveEventAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Преобразует постраничный результат мероприятий доменной модели
    /// в постраничный результат DTO ответа.
    /// </summary>
    /// <param name="result">Постраничный результат доменной модели.</param>
    /// <returns>Постраничный результат DTO ответа.</returns>
    private static PaginatedResult<EventResponse> PaginatedEventToResponse(PaginatedResult<EventDto> result)
        => new(
            result.Items.Select(DtoHelper.ToEventResponse),
            result.CurrentPage,
            result.PageSize,
            result.TotalPages,
            result.TotalItems,
            result.TotalItemsOnPage);
}

using EventFlow.Bookings.Api.Contracts;
using EventFlow.Bookings.Api.Contracts.Booking;
using EventFlow.Bookings.Application.Abstractions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

/// <summary>
/// Контроллер для получения информации о бронированиях.
/// </summary>
[Authorize]
[ApiController]
[Route("bookings")]
public class BookingController(IBookingService _bookingService) : ControllerBase
{
    /// <summary>
    /// Возвращает текущее состояние брони по её идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор брони.</param>
    /// <param name="ct">Токен отмены запроса.</param>
    /// <returns>Текущее состояние брони.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BookingResponse>> GetBooking(Guid id, CancellationToken ct)
    {
        var booking = await _bookingService.GetBookingByIdAsync(id, ct);
        return Ok(DtoHelper.ToBookingResponse(booking));
    }

    /// <summary>
    /// Создаёт бронь для мероприятия.
    /// Возвращает 202 Accepted, так как обработка брони выполняется фоновым сервисом.
    /// </summary>
    /// <param name="id">Идентификатор мероприятия.</param>
    /// <param name="ct">Токен отмены запроса.</param>
    /// <returns>Созданная бронь в статусе Pending.</returns>
    [Authorize]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [HttpPost("{id:guid}/book")]
    public async Task<ActionResult<BookingResponse>> CreateBooking(Guid id, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var booking = await _bookingService.CreateBookingAsync(id, userId, ct);
        var response = DtoHelper.ToBookingResponse(booking);

        return Accepted($"/bookings/{booking.Id}", response);
    }


    /// <summary>
    /// Отменяет бронь.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelBooking(Guid id, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdClaim, out var currentUserId))
        {
            return Unauthorized();
        }

        var isAdmin = User.IsInRole("Admin");

        await _bookingService.CancelBookingAsync(id, currentUserId, isAdmin, cancellationToken);

        return NoContent();
    }

}

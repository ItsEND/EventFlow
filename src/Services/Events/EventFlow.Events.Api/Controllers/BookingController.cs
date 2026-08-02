using EventFlow.Events.Api.Contracts;
using EventFlow.Events.Api.Contracts.Booking;
using EventFlow.Events.Application.Abstractions.Services;
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

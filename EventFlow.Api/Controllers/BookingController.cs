using EventFlow.Api.Contracts;
using EventFlow.Api.Contracts.Booking;
using EventFlow.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Контроллер для получения информации о бронированиях.
/// </summary>
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
    public async Task<ActionResult<BookingResponse>> GetBooking(Guid id, CancellationToken ct)
    {
        var booking = await _bookingService.GetBookingByIdAsync(id, ct);
        return Ok(DtoHelper.ToBookingResponse(booking));
    }
}
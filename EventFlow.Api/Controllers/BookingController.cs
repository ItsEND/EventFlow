using EventFlow.Api.Contracts;
using EventFlow.Api.Contracts.Booking;
using EventFlow.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventFlow.Api.Controllers
{
    [ApiController]
    [Route("bookings")]
    public class BookingController(IBookingService _bookingService) : ControllerBase
    {

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<BookingResponse>> GetBooking(Guid id, CancellationToken ct)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id, ct);
            return Ok(DtoHelper.ToBookingResponse(booking));
        }
    }
}

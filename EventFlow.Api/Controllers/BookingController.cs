using EventFlow.Api.Contracts;
using EventFlow.Api.Contracts.Booking;
using EventFlow.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventFlow.Api.Controllers
{
    [ApiController]
    [Route("bookings")]
    public class BookingController(IBookingService _bookingService, IBookingTaskQueue _taskQueue, ILogger<BookingController> _logger) : ControllerBase
    {
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<BookingResponse>> GetBooking(Guid id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            return Ok(DtoHelper.ToBookingResponse(booking));
        }
    }
}

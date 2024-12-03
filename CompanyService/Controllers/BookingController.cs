using CompanyService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CompanyService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private IBookingService bookingService;
        public BookingController(IBookingService bookingService)
        {
            this.bookingService = bookingService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetBookings()
        {
            var emailClaim = User?.FindFirst(ClaimTypes.Email)?.Value;
            if (emailClaim == null)
            {
                return Unauthorized();
            }

            var bookings = bookingService.GetBookingsAsync(emailClaim);

            return Ok();

        }
    }
}

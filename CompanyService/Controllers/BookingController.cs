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

            var bookings = await bookingService.GetWorkerBookingsAsync(emailClaim);

            return Ok(bookings);

        }
        [HttpGet("client")]
        [Authorize]
        public async Task<IActionResult> GetClientBookings()
        {
            var emailClaim = User?.FindFirst(ClaimTypes.Email)?.Value;
            if (emailClaim == null)
            {
                return Unauthorized();
            }

            var bookings = await bookingService.GetClientBookingsAsync(emailClaim);
            return Ok(bookings);
        }

        [HttpGet("company/{companyId}/statistics")]
        [Authorize]
        public async Task<IActionResult> GetCompanyBookingsStatistics(int companyId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            // Verify the user has permission to access this company's statistics
            var emailClaim = User?.FindFirst(ClaimTypes.Email)?.Value;
            if (emailClaim == null)
            {
                return Unauthorized();
            }


            var statistics = await bookingService.GetCompanyBookingsStatisticsAsync(companyId, startDate, endDate);
            return Ok(statistics);
        }
    }
}

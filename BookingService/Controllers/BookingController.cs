using BookingService.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using E = BookingService.Interfaces;
namespace BookingService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : Controller
    {
        private readonly E.IBookingService _bookingService;

        public BookingController(E.IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddBooking([FromBody] AddBookingDTO addBookingDTO)
        {
            var emailClaim = User?.FindFirst(ClaimTypes.Email)?.Value;

            await _bookingService.AddBookingAsync(
                addBookingDTO.BookingTimeLOC,
                addBookingDTO.WorkerId,
                emailClaim,
                addBookingDTO.ProductId,
                addBookingDTO.Duration
            );
            return Ok();
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditBooking(int id, [FromBody] EditBookingDTO editBookingDTO)
        {
            await _bookingService.EditBookingAsync(
                id,
                editBookingDTO.BookingTimeLOC,
                editBookingDTO.WorkerId
            );
            return Ok();
        }

        [Authorize]
        [HttpPatch("status/{id}")]
        public async Task<IActionResult> ChangeBookingStatus(int id, [FromBody] int newStatus)
        {
            await _bookingService.ChangeBookingStatusASync(id, newStatus);
            return Ok();
        }
    }
}


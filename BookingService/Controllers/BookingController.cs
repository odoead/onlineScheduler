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
        private readonly E.IBookingService bookingService;

        public BookingController(E.IBookingService bookingService)
        {
            this.bookingService = bookingService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddBooking([FromBody] AddBookingDTO addBookingDTO)
        {
            var emailClaim = User?.FindFirst(ClaimTypes.Email)?.Value;
            if (emailClaim == null)
            {
                return Unauthorized();
            }

            await bookingService.AddBookingAsync(
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
            await bookingService.EditBookingAsync(
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
            await bookingService.ChangeBookingStatusAsync(id, newStatus);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> test()
        {

            return Ok("99999999999999");
        }
        [HttpGet("rabbit")/*,Route("rabbit")*/]
        public async Task<IActionResult> testRabbit()
        {
            var data = await bookingService.GetRabbitDataTest();
            return Ok(data);
        }

    }
}


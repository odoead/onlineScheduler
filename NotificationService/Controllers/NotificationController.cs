using Microsoft.AspNetCore.Mvc;
using NotificationService.Interfaces;
using System.Security.Claims;

namespace NotificationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : Controller
    {
        private readonly INotificationService notificationService;

        public NotificationController(INotificationService notificationService)
        {
            this.notificationService = notificationService;
        }

        [HttpGet]
        [Route("GetNotifications")]
        public async Task<IActionResult> GetNotifications([FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            var emailClaim = User?.FindFirst(ClaimTypes.Email)?.Value;
            if (emailClaim == null)
            {
                return Unauthorized();
            }

            var notifications = await notificationService.GetNotifications(emailClaim, pageNumber, pageSize);
            return Ok(notifications);
        }
        //llll
    }
}

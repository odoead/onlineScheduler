using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Interfaces;
using System.Security.Claims;

namespace NotificationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationService notificationService;

        public NotificationController(INotificationService notificationService)
        {
            this.notificationService = notificationService;
        }

        [HttpGet]
        [Route("GetNotifications")]
        public async Task<IActionResult> GetNotifications([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var emailClaim = User?.FindFirst(ClaimTypes.Email)?.Value;
            if (emailClaim == null)
            {
                return Unauthorized();
            }

            var notifications = await notificationService.GetNotifications(emailClaim, pageNumber, pageSize);
            return Ok(notifications);
        }

        [HttpPost]
        [Route("MarkAsRead/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var emailClaim = User?.FindFirst(ClaimTypes.Email)?.Value;
            if (emailClaim == null)
            {
                return Unauthorized();
            }

            // Verify the notification belongs to this user
            var notifications = await notificationService.GetNotifications(emailClaim, 1, int.MaxValue);
            if (!notifications.DataList.Any(n => n.Id == id))
            {
                return NotFound("Notification not found or access denied");
            }

            await notificationService.MarkAsRead(id);
            return Ok();
        }
    }
}

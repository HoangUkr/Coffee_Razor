using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebUI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("admin/notifications")]
    public class AdminNotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public AdminNotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string scope = "today")
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var notifications = await _notificationService.GetForUserAsync(userId, pageNumber, pageSize, scope);
            return Ok(notifications);
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(new { unreadCount });
        }

        [HttpPost("{notificationId:int}/read")]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var marked = await _notificationService.MarkAsReadAsync(userId, notificationId);
            if (!marked)
            {
                return NotFound();
            }

            var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(new { unreadCount });
        }

        private Guid GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdValue, out var userId) ? userId : Guid.Empty;
        }
    }
}

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV.Service.Abstractions;

namespace ProjectFileStructure.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userGuid = User.FindFirst("UserGuid")?.Value;
            if (string.IsNullOrEmpty(userGuid)) return Unauthorized();

            var resp = await _notificationService.GetUserNotificationsAsync(userGuid);
            return Ok(resp);
        }

        [HttpPut("{notificationGuid}/read")]
        public async Task<IActionResult> MarkAsRead(string notificationGuid)
        {
            var resp = await _notificationService.MarkAsReadAsync(notificationGuid);
            if (!resp.Success)
            {
                return BadRequest(resp);
            }
            return Ok(resp);
        }

        [Authorize(Roles = "1")]
        [HttpPost("broadcast")]
        public async Task<IActionResult> Broadcast([FromQuery] string title, [FromQuery] string message)
        {
            var createdBy = User.FindFirst("UserGuid")?.Value ?? User.Identity?.Name ?? User.FindFirst("FullName")?.Value ?? "system";
            await _notificationService.BroadcastNotificationAsync(title, message, createdBy);
            return Ok(new { success = true, message = "Notification broadcasted." });
        }
    }
}

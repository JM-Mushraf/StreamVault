using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV.Service.Abstractions;

namespace ProjectFileStructure.Controllers;

[ApiController]
[Route("api/[controller]")]
/// <summary>
/// Watch history endpoints: insert watch history and retrieve paged history.
/// </summary>
public class WatchHistoryController : ControllerBase
{
    private readonly IWatchHistoryService _watchHistoryService;

    public WatchHistoryController(IWatchHistoryService watchHistoryService)
    {
        _watchHistoryService = watchHistoryService;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Insert([FromBody] SV.Common.DTOs.WatchHistoryCreateRequest request)
    {
        var userGuid = User.FindFirst("UserGuid")?.Value;
        if (string.IsNullOrWhiteSpace(userGuid)) return Unauthorized();

        var userStore = HttpContext.RequestServices.GetRequiredService<SV.Store.Abstractions.IUserStore>();
        var userId = await userStore.GetUserIdByGuidAsync(userGuid);
        if (userId == null) return NotFound(new { success = false, message = "User not found" });

        await _watchHistoryService.InsertWatchHistoryAsync(userId.Value, request.MovieId, request.WatchedDate, request.WatchMinutes, request.DeviceType);
        return Ok(new { success = true });
    }

    [Authorize]
    [HttpGet("paged")]
    public async Task<IActionResult> Paged([FromQuery] int userId, [FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        var items = await _watchHistoryService.GetWatchHistoryPagedAsync(userId, page, size);
        return Ok(items);
    }
}

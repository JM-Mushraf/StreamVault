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

        var profileGuid = Request.Headers["X-Profile-Guid"].ToString();
        if (string.IsNullOrWhiteSpace(profileGuid)) profileGuid = null;

        var createdBy = User.Identity?.Name ?? User.FindFirst("FullName")?.Value ?? "system";
        await _watchHistoryService.InsertWatchHistoryAsync(userGuid, request.MovieGuid, request.WatchedDate, request.WatchMinutes, request.DeviceType, createdBy, request.PlayheadSeconds, request.IsFinished, profileGuid);
        return Ok(new { success = true });
    }

    [Authorize]
    [HttpGet("paged")]
    public async Task<IActionResult> Paged([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        var userGuid = User.FindFirst("UserGuid")?.Value;
        if (string.IsNullOrWhiteSpace(userGuid)) return Unauthorized();

        var profileGuid = Request.Headers["X-Profile-Guid"].ToString();
        if (string.IsNullOrWhiteSpace(profileGuid)) profileGuid = null;

        var items = await _watchHistoryService.GetWatchHistoryPagedAsync(userGuid, page, size, profileGuid);
        return Ok(items);
    }

    [Authorize]
    [HttpGet("resume/{movieGuid}")]
    public async Task<IActionResult> GetResume(string movieGuid)
    {
        var userGuid = User.FindFirst("UserGuid")?.Value;
        if (string.IsNullOrWhiteSpace(userGuid)) return Unauthorized();

        var profileGuid = Request.Headers["X-Profile-Guid"].ToString();
        if (string.IsNullOrWhiteSpace(profileGuid)) profileGuid = null;

        var resume = await _watchHistoryService.GetResumeProgressAsync(userGuid, movieGuid, profileGuid);
        if (resume == null)
        {
            return NotFound(new { success = false, message = "No playback history found for this movie." });
        }

        return Ok(resume);
    }
}

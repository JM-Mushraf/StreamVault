using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SV.Service.Abstractions;

namespace ProjectFileStructure.Controllers;

[ApiController]
[Route("api/[controller]")]
/// <summary>
/// Watchlist endpoints: add/remove items and fetch user's watchlist.
/// </summary>
public class WatchlistController : ControllerBase
{
    private readonly IWatchlistService _watchlistService;

    public WatchlistController(IWatchlistService watchlistService)
    {
        _watchlistService = watchlistService;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] SV.Common.DTOs.WatchlistCreateRequest request)
    {
        var userGuid = User.FindFirst("UserGuid")?.Value;
        if (string.IsNullOrWhiteSpace(userGuid)) return Unauthorized();

        var profileGuid = Request.Headers["X-Profile-Guid"].ToString();
        if (string.IsNullOrWhiteSpace(profileGuid)) profileGuid = null;

        var createdBy = User.Identity?.Name ?? User.FindFirst("FullName")?.Value ?? "system";
        await _watchlistService.AddToWatchlistAsync(userGuid, request.MovieGuid, createdBy, profileGuid);
        return Ok(new { success = true });
    }

    [Authorize]
    [HttpDelete("{watchlistGuid}")]
    public async Task<IActionResult> Remove(string watchlistGuid)
    {
        var updatedBy = User.Identity?.Name ?? "SYSTEM";
        await _watchlistService.RemoveFromWatchlistAsync(watchlistGuid, updatedBy);
        return Ok(new { success = true });
    }

    [Authorize]
    [HttpGet("user/{userGuid}")]
    public async Task<IActionResult> GetUserWatchlist(string userGuid)
    {
        var profileGuid = Request.Headers["X-Profile-Guid"].ToString();
        if (string.IsNullOrWhiteSpace(profileGuid)) profileGuid = null;

        var items = await _watchlistService.GetUserWatchlistAsync(userGuid, profileGuid);
        return Ok(items);
    }
}

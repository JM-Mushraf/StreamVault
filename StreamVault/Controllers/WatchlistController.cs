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

        var userStore = HttpContext.RequestServices.GetRequiredService<SV.Store.Abstractions.IUserStore>();
        var userId = await userStore.GetUserIdByGuidAsync(userGuid);
        if (userId == null) return NotFound(new { success = false, message = "User not found" });

        await _watchlistService.AddToWatchlistAsync(userId.Value, request.MovieId);
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
        var items = await _watchlistService.GetUserWatchlistAsync(userGuid);
        return Ok(items);
    }
}

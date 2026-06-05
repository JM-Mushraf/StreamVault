using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV.Service.Abstractions;

namespace ProjectFileStructure.Controllers;

[ApiController]
[Route("api/[controller]")]
/// <summary>
/// Review endpoints: add reviews and fetch reviews for a movie.
/// </summary>
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] SV.Common.DTOs.ReviewCreateRequest request)
    {
        var userGuid = User.FindFirst("UserGuid")?.Value;
        if (string.IsNullOrWhiteSpace(userGuid))
            return Unauthorized();

        var userStoreObj = HttpContext.RequestServices.GetService(typeof(SV.Store.Abstractions.IUserStore));
        if (userStoreObj == null)
            return NotFound(new { success = false, message = "User store not available" });

        var userStore = (SV.Store.Abstractions.IUserStore)userStoreObj;
        var userId = await userStore.GetUserIdByGuidAsync(userGuid);
        if (userId == null)
            return NotFound(new { success = false, message = "User not found" });

        await _reviewService.AddReviewAsync(userId.Value, request.MovieId, request.Rating, request.ReviewText);
        return Ok(new { success = true });
    }

    [HttpGet("movie/{movieGuid}")]
    public async Task<IActionResult> GetByMovie(string movieGuid)
    {
        var items = await _reviewService.GetMovieReviewsAsync(movieGuid);
        return Ok(items);
    }
}

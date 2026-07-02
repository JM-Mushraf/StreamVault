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

        var profileGuid = Request.Headers["X-Profile-Guid"].ToString();
        if (string.IsNullOrWhiteSpace(profileGuid)) profileGuid = null;

        var createdBy = User.Identity?.Name ?? User.FindFirst("FullName")?.Value ?? User.FindFirst("email")?.Value ?? string.Empty;

        await _reviewService.AddReviewAsync(userGuid, request.MovieGuid, request.Rating, request.ReviewText, createdBy, profileGuid);
        return Ok(new { success = true });
    }

    [HttpGet("movie/{movieGuid}")]
    public async Task<IActionResult> GetByMovie(string movieGuid)
    {
        var result = await _reviewService.GetByMovieGuidAsync(movieGuid);
        return Ok(result);
    }
}

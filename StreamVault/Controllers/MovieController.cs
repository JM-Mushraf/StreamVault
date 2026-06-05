using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV.Common.DTOs;
using SV.Service.Abstractions;

namespace ProjectFileStructure.Controllers;

[ApiController]
[Route("api/[controller]")]
/// <summary>
/// Movie endpoints: fetch latest movies and create new movie entries (admin only).
/// </summary>
public class MovieController : ControllerBase
{
    private readonly IMovieService _movieService;

    public MovieController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatest()
    {
        var movies = await _movieService.GetLatestMoviesAsync();
        return Ok(movies);
    }

    [Authorize(Roles = "1")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMovieRequest request)
    {
        await _movieService.CreateMovieAsync(request);
        return Ok(new { success = true });
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV.Service.Abstractions;

namespace ProjectFileStructure.Controllers;

[ApiController]
[Route("api/[controller]")]
/// <summary>
/// Genre endpoints: list genres and create new genres (admin only).
/// </summary>
public class GenreController : ControllerBase
{
    private readonly IGenreService _genreService;

    public GenreController(IGenreService genreService)
    {
        _genreService = genreService;
    }

    // GET /api/Genre - authenticated users
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var genres = await _genreService.GetGenresAsync();
        return Ok(genres);
    }

    // POST /api/Genre - admin only (role = 1)
    [Authorize(Roles = "1")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SV.Common.DTOs.Genre.CreateGenreDto request)
    {
        await _genreService.CreateGenreAsync(request.Name, "system");
        return Ok(new { success = true });
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV.Common.DTOs.Movie;
using SV.Service.Abstractions;
using SV.Common.DTOs;

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
    public async Task<IActionResult> GetLatest([FromQuery(Name = "genres")] string[]? genres)
    {
        var movies = await _movieService.GetLatestMoviesAsync(genres);
        return Ok(movies);
    }

    [Authorize]
    [HttpGet("by-genres")]
    public async Task<IActionResult> GetByGenresPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery(Name = "genres")] string[]? genres = null)
    {
        var paged = await _movieService.GetByGenresPagedAsync(pageNumber, pageSize, genres);
        return Ok(paged);
    }

    [HttpGet("trending")]
    public async Task<IActionResult> GetTrendingPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var paged = await _movieService.GetTrendingMoviesPagedAsync(pageNumber, pageSize);
        return Ok(paged);
    }

    [HttpGet("suggest")]
    public async Task<IActionResult> Suggest([FromQuery] string q)
    {
        var suggestions = await _movieService.GetSuggestionsAsync(q);
        return Ok(suggestions);
    }



    [Authorize(Roles = "1")]
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ApiExplorerSettings(IgnoreApi = false)]
    public async Task<IActionResult> Create()
    {
        try
        {
            var createdBy = User.FindFirst("UserGuid")?.Value ?? User.Identity?.Name ?? User.FindFirst("FullName")?.Value ?? "system";

            IFormCollection form = null;
            try
            {
                // Access Request.Form directly - it's lazy-loaded and will use our FormOptions
                // This is safer than calling ReadFormAsync()
                form = Request.Form;
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    error = $"Failed to access form: {ex.Message}",
                    type = ex.GetType().Name
                });
            }

            if (form == null || form.Count == 0)
            {
                return BadRequest(new { error = "Form data is empty" });
            }

            // Parse form fields
            var genreGuid = form["GenreGuid"].ToString();
            var movieName = form["MovieName"].ToString();
            var durationStr = form["DurationMinutes"].ToString();
            var releaseDate = form["ReleaseDate"].ToString();
            var language = form["Language"].ToString();
            var ratingStr = form["Rating"].ToString();

            if (string.IsNullOrWhiteSpace(genreGuid) || string.IsNullOrWhiteSpace(movieName) || 
                string.IsNullOrWhiteSpace(durationStr) || string.IsNullOrWhiteSpace(releaseDate) ||
                string.IsNullOrWhiteSpace(language))
            {
                return BadRequest(new { error = "Missing required fields: GenreGuid, MovieName, DurationMinutes, ReleaseDate, Language" });
            }

            if (!int.TryParse(durationStr, out var duration))
                return BadRequest(new { error = "DurationMinutes must be a valid integer" });

            if (!DateTime.TryParse(releaseDate, out var releaseDateParsed))
                return BadRequest(new { error = "ReleaseDate must be a valid date (format: YYYY-MM-DD)" });

            int? rating = null;
            if (!string.IsNullOrWhiteSpace(ratingStr) && int.TryParse(ratingStr, out var parsedRating))
                rating = parsedRating;

            var request = new CreateMovieRequest
            {
                GenreGuid = genreGuid,
                MovieName = movieName,
                DurationMinutes = duration,
                ReleaseDate = releaseDateParsed,
                Language = language,
                Rating = rating
            };

            SV.Common.DTOs.FileUploadDto? videoDto = null;
            SV.Common.DTOs.FileUploadDto? thumbDto = null;

            var videoFile = form.Files["video"];
            var thumbnailFile = form.Files["thumbnail"];

            // 1. Validate files (Format and Size)
            if (videoFile != null && videoFile.Length > 0)
            {
                var videoVal = ProjectFileStructure.Helpers.FileValidator.ValidateVideo(videoFile, "Video file");
                if (!videoVal.IsValid)
                {
                    return BadRequest(new { error = videoVal.ErrorMessage });
                }
            }

            if (thumbnailFile != null && thumbnailFile.Length > 0)
            {
                var thumbVal = ProjectFileStructure.Helpers.FileValidator.ValidateImage(thumbnailFile, "Thumbnail file");
                if (!thumbVal.IsValid)
                {
                    return BadRequest(new { error = thumbVal.ErrorMessage });
                }
            }

            // 2. Prepare FileUploadDto using the direct read streams (Memory-efficient)
            if (videoFile != null && videoFile.Length > 0)
            {
                videoDto = new SV.Common.DTOs.FileUploadDto 
                { 
                    Content = videoFile.OpenReadStream(), 
                    FileName = videoFile.FileName, 
                    ContentType = videoFile.ContentType 
                };
            }

            if (thumbnailFile != null && thumbnailFile.Length > 0)
            {
                thumbDto = new SV.Common.DTOs.FileUploadDto 
                { 
                    Content = thumbnailFile.OpenReadStream(), 
                    FileName = thumbnailFile.FileName, 
                    ContentType = thumbnailFile.ContentType 
                };
            }

            var movieGuid = await _movieService.CreateMovieAsync(request, createdBy, videoDto, thumbDto);
            if (string.IsNullOrWhiteSpace(movieGuid)) 
                return StatusCode(500, new { error = "Failed to create movie" });

            return CreatedAtAction(nameof(GetByGuid), new { movieGuid }, new { movieGuid });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Server error: {ex.Message}" });
        }
    }

    [HttpGet("{movieGuid}")]
    public async Task<IActionResult> GetByGuid(string movieGuid)
    {
        var movie = await _movieService.GetByGuidAsync(movieGuid);
        if (movie == null) return NotFound();
        return Ok(movie);
    }

    [Authorize(Roles = "1")]
    [HttpPut("{movieGuid}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(string movieGuid)
    {
        try
        {
            var updatedBy = User.FindFirst("UserGuid")?.Value ?? User.Identity?.Name ?? User.FindFirst("FullName")?.Value ?? "system";

            IFormCollection form = null;
            try
            {
                form = Request.Form;
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    error = $"Failed to access form: {ex.Message}",
                    type = ex.GetType().Name
                });
            }

            if (form == null || (form.Count == 0 && form.Files.Count == 0))
            {
                return BadRequest(new { error = "Form data is empty" });
            }

            // Parse optional form fields
            var genreGuid = form.ContainsKey("GenreGuid") ? form["GenreGuid"].ToString() : null;
            var movieName = form.ContainsKey("MovieName") ? form["MovieName"].ToString() : null;
            var durationStr = form.ContainsKey("DurationMinutes") ? form["DurationMinutes"].ToString() : null;
            var language = form.ContainsKey("Language") ? form["Language"].ToString() : null;
            var ratingStr = form.ContainsKey("Rating") ? form["Rating"].ToString() : null;

            int? duration = null;
            if (!string.IsNullOrWhiteSpace(durationStr))
            {
                if (!int.TryParse(durationStr, out var parsedDuration))
                    return BadRequest(new { error = "DurationMinutes must be a valid integer" });
                duration = parsedDuration;
            }

            int? rating = null;
            if (!string.IsNullOrWhiteSpace(ratingStr))
            {
                if (!int.TryParse(ratingStr, out var parsedRating))
                    return BadRequest(new { error = "Rating must be a valid integer" });
                rating = parsedRating;
            }

            var request = new UpdateMovieRequest
            {
                GenreGuid = genreGuid,
                MovieName = movieName,
                DurationMinutes = duration,
                Language = language,
                Rating = rating
            };

            var videoFile = form.Files["video"];
            var thumbnailFile = form.Files["thumbnail"];

            // Validate files if they are provided
            if (videoFile != null && videoFile.Length > 0)
            {
                var videoVal = ProjectFileStructure.Helpers.FileValidator.ValidateVideo(videoFile, "Video file");
                if (!videoVal.IsValid)
                {
                    return BadRequest(new { error = videoVal.ErrorMessage });
                }
            }

            if (thumbnailFile != null && thumbnailFile.Length > 0)
            {
                var thumbVal = ProjectFileStructure.Helpers.FileValidator.ValidateImage(thumbnailFile, "Thumbnail file");
                if (!thumbVal.IsValid)
                {
                    return BadRequest(new { error = thumbVal.ErrorMessage });
                }
            }

            SV.Common.DTOs.FileUploadDto? videoDto = null;
            SV.Common.DTOs.FileUploadDto? thumbDto = null;

            if (videoFile != null && videoFile.Length > 0)
            {
                videoDto = new SV.Common.DTOs.FileUploadDto 
                { 
                    Content = videoFile.OpenReadStream(), 
                    FileName = videoFile.FileName, 
                    ContentType = videoFile.ContentType 
                };
            }

            if (thumbnailFile != null && thumbnailFile.Length > 0)
            {
                thumbDto = new SV.Common.DTOs.FileUploadDto 
                { 
                    Content = thumbnailFile.OpenReadStream(), 
                    FileName = thumbnailFile.FileName, 
                    ContentType = thumbnailFile.ContentType 
                };
            }

            var success = await _movieService.UpdateMovieAsync(movieGuid, request, updatedBy, videoDto, thumbDto);
            if (!success)
            {
                return NotFound(new { error = "Movie not found or failed to update" });
            }

            var updatedMovie = await _movieService.GetByGuidAsync(movieGuid);
            return Ok(updatedMovie);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Server error: {ex.Message}" });
        }
    }

    //[Authorize(Roles = "1")]
    //[HttpPost("{movieGuid}/video")]
    //[Consumes("multipart/form-data")]
    //public async Task<IActionResult> UploadVideo(string movieGuid)
    //{
    //    var file = Request?.Form?.Files?.FirstOrDefault();
    //    if (file == null) return BadRequest("file is required");

    //    using var ms = new System.IO.MemoryStream();
    //    await file.CopyToAsync(ms);
    //    ms.Position = 0;

    //    var uploadDto = new FileUploadDto
    //    {
    //        Content = ms,
    //        FileName = file.FileName,
    //        ContentType = file.ContentType
    //    };

    //    var res = await _movieService.UploadMovieVideoAsync(movieGuid, uploadDto);
    //    if (res == null) return StatusCode(500);
    //    return Accepted(res);
    //}

    //[Authorize(Roles = "1")]
    //[HttpPost("{movieGuid}/thumbnail")]
    //[Consumes("multipart/form-data")]
    //public async Task<IActionResult> UploadThumbnail(string movieGuid)
    //{
    //    var file = Request?.Form?.Files?.FirstOrDefault();
    //    if (file == null) return BadRequest("file is required");

    //    using var ms = new System.IO.MemoryStream();
    //    await file.CopyToAsync(ms);
    //    ms.Position = 0;

    //    var uploadDto = new FileUploadDto
    //    {
    //        Content = ms,
    //        FileName = file.FileName,
    //        ContentType = file.ContentType
    //    };

    //    var res = await _movieService.UploadMovieThumbnailAsync(movieGuid, uploadDto);
    //    if (res == null) return StatusCode(500);
    //    return Ok(res);
    //}
}

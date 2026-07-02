using SV.Common.DTOs.Movie;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SV.Store.Abstractions
{
    public interface IMovieStore
    {
        Task<List<object>> GetLatestMoviesAsync(string[]? genres = null);
        Task<SV.Common.Models.PagedResult<SV.Common.DTOs.Movie.MovieResponseDto>> GetByGenresPagedAsync(int pageNumber, int pageSize, string[]? genres = null);
        Task<SV.Common.Models.PagedResult<SV.Common.DTOs.Movie.MovieResponseDto>> GetTrendingMoviesPagedAsync(int pageNumber, int pageSize);
        Task<string> CreateMovieAsync(CreateMovieRequest request, string createdBy, string? movieVideoUrl = null, string? movieVideoPublicId = null, string? videoTranscodeStatus = null, string? availableFormats = null, string? thumbnailUrl = null, string? thumbnailPublicId = null);
        Task<object?> GetByGuidAsync(string movieGuid);
        Task<bool> UpdateMovieVideoMetaAsync(string movieGuid, string videoUrl, string videoPublicId, string? transcodeStatus = null, string? availableFormats = null);
        Task<bool> UpdateMovieThumbnailAsync(string movieGuid, string thumbnailUrl, string thumbnailPublicId);
        Task<bool> UpdateMovieAsync(
            string movieGuid, 
            string? genreGuid, 
            string? movieName, 
            int? durationMinutes, 
            string? language, 
            int? rating, 
            string updatedBy, 
            string? movieVideoUrl = null, 
            string? movieVideoPublicId = null, 
            string? videoTranscodeStatus = null, 
            string? availableFormats = null, 
            string? thumbnailUrl = null, 
            string? thumbnailPublicId = null);
        Task<List<MovieSuggestResponseDto>> GetSuggestionsAsync(string query);
    }
}

using SV.Common.DTOs.Movie;
using SV.Common.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using SV.Service.Abstractions;

namespace SV.Service.Abstractions
{
    public interface IMovieService
    {
        Task<List<object>> GetLatestMoviesAsync(string[]? genres = null);
        Task<SV.Common.Models.PagedResult<SV.Common.DTOs.Movie.MovieResponseDto>> GetByGenresPagedAsync(int pageNumber, int pageSize, string[]? genres = null);
        Task<SV.Common.Models.PagedResult<SV.Common.DTOs.Movie.MovieResponseDto>> GetTrendingMoviesPagedAsync(int pageNumber, int pageSize);
        Task<string> CreateMovieAsync(CreateMovieRequest request, string createdBy, SV.Common.DTOs.FileUploadDto? video = null, SV.Common.DTOs.FileUploadDto? thumbnail = null);
        Task<object?> GetByGuidAsync(string movieGuid);
        Task<object?> UploadMovieVideoAsync(string movieGuid, FileUploadDto file);
        Task<object?> UploadMovieThumbnailAsync(string movieGuid, FileUploadDto file);
        Task<bool> UpdateMovieAsync(
            string movieGuid, 
            UpdateMovieRequest request, 
            string updatedBy, 
            SV.Common.DTOs.FileUploadDto? video = null, 
            SV.Common.DTOs.FileUploadDto? thumbnail = null);
        Task<List<MovieSuggestResponseDto>> GetSuggestionsAsync(string query);
    }
}

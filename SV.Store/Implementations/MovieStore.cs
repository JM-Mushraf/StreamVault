using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using SV.Store.Abstractions;
using SV.Data.Connections;
using SV.Common.Constants;
using SV.Common.DTOs.Movie;

namespace SV.Store.Implementations
{
    public class MovieStore : IMovieStore
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public MovieStore(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<List<object>> GetLatestMoviesAsync(string[]? genres = null)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            var csv = genres == null ? null : string.Join(',', genres.Select(g => g.Trim()));

            var rows = await conn.QueryAsync<dynamic>(
                AppConstants.SpGetLatestMovies,
                new { GenreNamesCsv = csv },
                commandType: CommandType.StoredProcedure);

            return rows.Select(r => (object)r).ToList();
        }

        public async Task<object?> GetByGuidAsync(string movieGuid)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            var row = await conn.QueryFirstOrDefaultAsync<dynamic>(AppConstants.SpGetMovieByGuid, new { MovieGuid = movieGuid }, commandType: CommandType.StoredProcedure);
            return row == null ? null : (object)row;
        }

        public async Task<string> CreateMovieAsync(CreateMovieRequest request, string createdBy, string? movieVideoUrl = null, string? movieVideoPublicId = null, string? videoTranscodeStatus = null, string? availableFormats = null, string? thumbnailUrl = null, string? thumbnailPublicId = null)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            var parameters = new DynamicParameters();
            parameters.Add("@GenreGuid", request.GenreGuid);
            parameters.Add("@MovieName", request.MovieName);
            parameters.Add("@DurationMinutes", request.DurationMinutes);
            parameters.Add("@ReleaseDate", request.ReleaseDate);
            parameters.Add("@Language", request.Language);
            parameters.Add("@Rating", request.Rating);
            parameters.Add("@CreatedBy", createdBy);

            // Media fields (optional)
            parameters.Add("@MovieVideoUrl", movieVideoUrl);
            parameters.Add("@MovieVideoPublicId", movieVideoPublicId);
            parameters.Add("@VideoTranscodeStatus", videoTranscodeStatus);
            parameters.Add("@AvailableFormats", availableFormats);
            parameters.Add("@ThumbnailUrl", thumbnailUrl);
            parameters.Add("@ThumbnailPublicId", thumbnailPublicId);

            // usp_InsertMovie now returns MovieGuid as single-column result
            var result = await conn.QueryFirstOrDefaultAsync<string>(
                AppConstants.SpInsertMovie,
                parameters,
                commandType: CommandType.StoredProcedure);

            return result ?? string.Empty;
        }

        public async Task<bool> UpdateMovieVideoMetaAsync(string movieGuid, string videoUrl, string videoPublicId, string? transcodeStatus = null, string? availableFormats = null)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            var parameters = new DynamicParameters();
            parameters.Add("@MovieGuid", movieGuid);
            parameters.Add("@MovieVideoUrl", videoUrl);
            parameters.Add("@MovieVideoPublicId", videoPublicId);
            parameters.Add("@VideoTranscodeStatus", transcodeStatus);
            parameters.Add("@AvailableFormats", availableFormats);

            try
            {
                await conn.ExecuteAsync("dbo.usp_UpdateMovie", parameters, commandType: CommandType.StoredProcedure);
                return true;
            }
            catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 50003)
            {
                return false;
            }
        }

        public async Task<bool> UpdateMovieThumbnailAsync(string movieGuid, string thumbnailUrl, string thumbnailPublicId)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            var parameters = new DynamicParameters();
            parameters.Add("@MovieGuid", movieGuid);
            parameters.Add("@ThumbnailUrl", thumbnailUrl);
            parameters.Add("@ThumbnailPublicId", thumbnailPublicId);

            try
            {
                await conn.ExecuteAsync("dbo.usp_UpdateMovie", parameters, commandType: CommandType.StoredProcedure);
                return true;
            }
            catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 50003)
            {
                return false;
            }
        }

        public async Task<bool> UpdateMovieAsync(
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
            string? thumbnailPublicId = null)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            int? genreId = null;
            if (!string.IsNullOrWhiteSpace(genreGuid))
            {
                genreId = await conn.QueryFirstOrDefaultAsync<int?>(
                    "SELECT GenreId FROM mst_Genre WHERE GenreGuid = @GenreGuid AND IsActive = 1", 
                    new { GenreGuid = genreGuid });
                if (genreId == null)
                {
                    throw new System.Exception("Invalid GenreGuid provided.");
                }
            }

            var parameters = new DynamicParameters();
            parameters.Add("@MovieGuid", movieGuid);
            parameters.Add("@GenreId", genreId);
            parameters.Add("@MovieName", movieName);
            parameters.Add("@DurationMinutes", durationMinutes);
            parameters.Add("@Language", language);
            parameters.Add("@Rating", rating);
            parameters.Add("@UpdatedBy", updatedBy);
            parameters.Add("@MovieVideoUrl", movieVideoUrl);
            parameters.Add("@MovieVideoPublicId", movieVideoPublicId);
            parameters.Add("@VideoTranscodeStatus", videoTranscodeStatus);
            parameters.Add("@AvailableFormats", availableFormats);
            parameters.Add("@ThumbnailUrl", thumbnailUrl);
            parameters.Add("@ThumbnailPublicId", thumbnailPublicId);

            try
            {
                await conn.ExecuteAsync("dbo.usp_UpdateMovie", parameters, commandType: CommandType.StoredProcedure);
                return true;
            }
            catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 50003)
            {
                return false;
            }
        }

        public async Task<SV.Common.Models.PagedResult<SV.Common.DTOs.Movie.MovieResponseDto>> GetByGenresPagedAsync(int pageNumber, int pageSize, string[]? genres = null)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            var csv = genres == null ? null : string.Join(',', genres.Select(g => g.Trim()));

            var items = await conn.QueryAsync<dynamic>(
                "usp_GetMoviesPaged",
                new { PageNumber = pageNumber, PageSize = pageSize, GenreNamesCsv = csv },
                commandType: CommandType.StoredProcedure);

            var result = new SV.Common.Models.PagedResult<SV.Common.DTOs.Movie.MovieResponseDto>();

            var movieList = items.Select(r => new SV.Common.DTOs.Movie.MovieResponseDto
            {
                MovieGuid = r.MovieGuid ?? string.Empty,
                MovieName = r.MovieName ?? string.Empty,
                ReleaseDate = r.ReleaseDate ?? DateTime.MinValue,
                DurationMinutes = r.DurationMinutes ?? 0,
                Language = r.Language ?? string.Empty,
                Rating = r.Rating,
                GenreName = r.GenreName ?? string.Empty,
                CreatedOn = r.CreatedOn ?? DateTime.MinValue,
                CreatedBy = r.CreatedBy ?? string.Empty,
                ThumbnailUrl = r.ThumbnailUrl,
                ThumbnailPublicId = r.ThumbnailPublicId,
                MovieVideoUrl = r.MovieVideoUrl,
                MovieVideoPublicId = r.MovieVideoPublicId,
                VideoTranscodeStatus = r.VideoTranscodeStatus,
                AvailableFormats = r.AvailableFormats
            }).ToList();

            result.Items = movieList;
            result.TotalCount = movieList.Count;
            return result;
        }

        public async Task<SV.Common.Models.PagedResult<SV.Common.DTOs.Movie.MovieResponseDto>> GetTrendingMoviesPagedAsync(int pageNumber, int pageSize)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            var items = await conn.QueryAsync<dynamic>(
                AppConstants.SpGetTrendingMovies,
                new { PageNumber = pageNumber, PageSize = pageSize },
                commandType: CommandType.StoredProcedure);

            var result = new SV.Common.Models.PagedResult<SV.Common.DTOs.Movie.MovieResponseDto>();

            var movieList = items.Select(r => new SV.Common.DTOs.Movie.MovieResponseDto
            {
                MovieGuid = r.MovieGuid ?? string.Empty,
                MovieName = r.MovieName ?? string.Empty,
                ReleaseDate = r.ReleaseDate ?? DateTime.MinValue,
                DurationMinutes = r.DurationMinutes ?? 0,
                Language = r.Language ?? string.Empty,
                Rating = r.Rating,
                GenreName = r.GenreName ?? string.Empty,
                CreatedOn = r.CreatedOn ?? DateTime.MinValue,
                CreatedBy = r.CreatedBy ?? string.Empty,
                ThumbnailUrl = r.ThumbnailUrl,
                ThumbnailPublicId = r.ThumbnailPublicId,
                MovieVideoUrl = r.MovieVideoUrl,
                MovieVideoPublicId = r.MovieVideoPublicId,
                VideoTranscodeStatus = r.VideoTranscodeStatus,
                AvailableFormats = r.AvailableFormats
            }).ToList();

            result.Items = movieList;
            // Get TotalCount from the first record if available (returned via COUNT(*) OVER())
            result.TotalCount = items.FirstOrDefault()?.TotalCount ?? 0;
            return result;
        }

        public async Task<List<MovieSuggestResponseDto>> GetSuggestionsAsync(string query)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            string sql = @"
                SELECT 
                    m.MovieGuid, m.MovieName, m.ThumbnailUrl, g.GenreName
                FROM mst_Movie m
                INNER JOIN mst_Genre g ON m.GenreId = g.GenreId
                WHERE m.IsActive = 1 AND (m.MovieName LIKE @Query OR g.GenreName LIKE @Query OR m.Language LIKE @Query)";

            var rows = await conn.QueryAsync<MovieSuggestResponseDto>(sql, new { Query = "%" + query + "%" });
            return rows.ToList();
        }
    }
}
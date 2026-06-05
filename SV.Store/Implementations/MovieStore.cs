using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using SV.Store.Abstractions;
using SV.Data.Connections;

namespace SV.Store.Implementations
{
    public class MovieStore : IMovieStore
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public MovieStore(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public Task<List<object>> GetLatestMoviesAsync()
        {
            return Task.FromResult(new List<object>());
        }

        public async Task CreateMovieAsync(SV.Common.DTOs.CreateMovieRequest request)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            var parameters = new
            {
                GenreId = request.GenreId,
                MovieName = request.MovieName,
                DurationMinutes = request.DurationMinutes,
                ReleaseDate = request.ReleaseDate,
                Language = request.Language,
                Rating = request.Rating,
                CreatedBy = "system"
            };

            await conn.ExecuteAsync("usp_InsertMovie", parameters, commandType: CommandType.StoredProcedure);
        }
    }
}
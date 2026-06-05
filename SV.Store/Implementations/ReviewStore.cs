using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using System.Linq;
using SV.Store.Abstractions;
using SV.Data.Connections;
using SV.Common.Constants;

namespace SV.Store.Implementations
{
    public class ReviewStore : IReviewStore
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ReviewStore(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task AddAsync(int userId, int movieId, decimal rating, string? reviewText)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            await conn.ExecuteAsync(AppConstants.SpAddReview, new { UserId = userId, MovieId = movieId, Rating = rating, ReviewText = reviewText, CreatedBy = "system" }, commandType: CommandType.StoredProcedure);
        }

        public async Task<List<object>> GetByMovieAsync(string movieGuid)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            var rows = await conn.QueryAsync<dynamic>(AppConstants.SpGetMovieReviews, new { MovieGuid = movieGuid }, commandType: CommandType.StoredProcedure);
            return rows.Select(r => (object)r).ToList();
        }
    }
}

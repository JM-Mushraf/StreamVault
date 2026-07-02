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

        public async Task AddAsync(string userGuid, string movieGuid, int rating, string? reviewText, string createdBy, string? profileGuid = null)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            var userId = await conn.QueryFirstOrDefaultAsync<int?>("SELECT UserId FROM mst_User WHERE UserGuid = @UserGuid", new { UserGuid = userGuid });
            var movieId = await conn.QueryFirstOrDefaultAsync<int?>("SELECT MovieId FROM mst_Movie WHERE MovieGuid = @MovieGuid", new { MovieGuid = movieGuid });

            if (userId == null || movieId == null) return;

            string query = @"
                INSERT INTO tbl_Review (ReviewGuid, UserId, MovieId, Rating, ReviewText, IsActive, CreatedOn, CreatedBy, ProfileGuid)
                VALUES (@Guid, @UserId, @MovieId, @Rating, @ReviewText, 1, GETDATE(), @CreatedBy, @ProfileGuid)";

            await conn.ExecuteAsync(query, new {
                Guid = System.Guid.NewGuid().ToString("N"),
                UserId = userId,
                MovieId = movieId,
                Rating = rating,
                ReviewText = reviewText,
                CreatedBy = createdBy,
                ProfileGuid = profileGuid
            });
        }

        public async Task<List<object>> GetByMovieAsync(string movieGuid)
        {
            if (string.IsNullOrWhiteSpace(movieGuid))
            {
                return new List<object>();
            }

            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            var normalizedGuid = movieGuid.Trim();
            string query = @"
                SELECT 
                    r.ReviewGuid, r.Rating, r.ReviewText, r.CreatedOn, r.ProfileGuid,
                    COALESCE(p.ProfileName, u.FullName) as UserFullName
                FROM tbl_Review r
                INNER JOIN mst_Movie m ON r.MovieId = m.MovieId
                INNER JOIN mst_User u ON r.UserId = u.UserId
                LEFT JOIN mst_Profile p ON r.ProfileGuid = p.ProfileGuid AND p.IsActive = 1
                WHERE m.MovieGuid = @MovieGuid AND r.IsActive = 1
                ORDER BY r.CreatedOn DESC";

            var rows = await conn.QueryAsync<dynamic>(query, new { MovieGuid = normalizedGuid });
            return rows.Select(r => (object)r).ToList();
        }
    }
}


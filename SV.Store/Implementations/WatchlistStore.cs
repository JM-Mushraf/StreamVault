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
    public class WatchlistStore : IWatchlistStore
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public WatchlistStore(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task AddAsync(string userGuid, string movieGuid, string createdBy, string? profileGuid = null)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            var userId = await conn.QueryFirstOrDefaultAsync<int?>("SELECT UserId FROM mst_User WHERE UserGuid = @UserGuid", new { UserGuid = userGuid });
            var movieId = await conn.QueryFirstOrDefaultAsync<int?>("SELECT MovieId FROM mst_Movie WHERE MovieGuid = @MovieGuid", new { MovieGuid = movieGuid });

            if (userId == null || movieId == null) return;

            string checkQuery = profileGuid != null
                ? "SELECT WatchlistId FROM tbl_Watchlist WHERE UserId = @UserId AND MovieId = @MovieId AND ProfileGuid = @ProfileGuid AND IsActive = 1"
                : "SELECT WatchlistId FROM tbl_Watchlist WHERE UserId = @UserId AND MovieId = @MovieId AND ProfileGuid IS NULL AND IsActive = 1";
            
            var existingId = await conn.QueryFirstOrDefaultAsync<int?>(checkQuery, new { UserId = userId, MovieId = movieId, ProfileGuid = profileGuid });

            if (existingId == null)
            {
                string insertQuery = @"
                    INSERT INTO tbl_Watchlist (WatchlistGuid, UserId, MovieId, AddedOn, IsActive, CreatedOn, CreatedBy, ProfileGuid)
                    VALUES (@Guid, @UserId, @MovieId, GETDATE(), 1, GETDATE(), @CreatedBy, @ProfileGuid)";

                await conn.ExecuteAsync(insertQuery, new {
                    Guid = System.Guid.NewGuid().ToString("N"),
                    UserId = userId,
                    MovieId = movieId,
                    CreatedBy = createdBy,
                    ProfileGuid = profileGuid
                });
            }
        }

        public async Task<List<object>> GetByUserAsync(string userGuid, string? profileGuid = null)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            string query = @"
                SELECT 
                    w.WatchlistGuid, w.AddedOn, w.ProfileGuid,
                    m.MovieGuid, m.MovieName, m.DurationMinutes, m.Language, m.ThumbnailUrl, m.ThumbnailPublicId
                FROM tbl_Watchlist w
                INNER JOIN mst_User u ON w.UserId = u.UserId
                INNER JOIN mst_Movie m ON w.MovieId = m.MovieId
                WHERE u.UserGuid = @UserGuid AND w.IsActive = 1 " +
                (profileGuid != null ? "AND w.ProfileGuid = @ProfileGuid " : "AND w.ProfileGuid IS NULL ") +
                "ORDER BY w.AddedOn DESC";

            var rows = await conn.QueryAsync<dynamic>(query, new { UserGuid = userGuid, ProfileGuid = profileGuid });
            return rows.Select(r => (object)r).ToList();
        }

        public async Task RemoveAsync(string watchlistGuid, string updatedBy)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            await conn.ExecuteAsync("UPDATE tbl_Watchlist SET IsActive = 0, UpdatedOn = GETDATE(), UpdatedBy = @UpdatedBy WHERE WatchlistGuid = @WatchlistGuid", new { WatchlistGuid = watchlistGuid, UpdatedBy = updatedBy });
        }
    }
}

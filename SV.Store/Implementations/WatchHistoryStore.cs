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
    public class WatchHistoryStore : IWatchHistoryStore
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public WatchHistoryStore(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task InsertAsync(string userGuid, string movieGuid, System.DateTime watchedDate, int watchMinutes, string deviceType, string createdBy, int playheadSeconds = 0, bool isFinished = false, string? profileGuid = null)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            var userId = await conn.QueryFirstOrDefaultAsync<int?>("SELECT UserId FROM mst_User WHERE UserGuid = @UserGuid", new { UserGuid = userGuid });
            var movieId = await conn.QueryFirstOrDefaultAsync<int?>("SELECT MovieId FROM mst_Movie WHERE MovieGuid = @MovieGuid", new { MovieGuid = movieGuid });

            if (userId == null || movieId == null) return;

            string checkQuery = profileGuid != null 
                ? "SELECT WatchHistoryId FROM tbl_WatchHistory WHERE UserId = @UserId AND MovieId = @MovieId AND ProfileGuid = @ProfileGuid AND IsActive = 1"
                : "SELECT WatchHistoryId FROM tbl_WatchHistory WHERE UserId = @UserId AND MovieId = @MovieId AND ProfileGuid IS NULL AND IsActive = 1";
            
            var existingHistoryId = await conn.QueryFirstOrDefaultAsync<int?>(checkQuery, new { UserId = userId, MovieId = movieId, ProfileGuid = profileGuid });

            if (existingHistoryId != null)
            {
                string updateQuery = "UPDATE tbl_WatchHistory SET WatchedDate = @WatchedDate, WatchMinutes = WatchMinutes + @WatchMinutes, DeviceType = @DeviceType, PlayheadSeconds = @PlayheadSeconds, IsFinished = @IsFinished, UpdatedOn = GETDATE(), UpdatedBy = @CreatedBy WHERE WatchHistoryId = @HistoryId";
                await conn.ExecuteAsync(updateQuery, new { WatchedDate = watchedDate, WatchMinutes = watchMinutes, DeviceType = deviceType, PlayheadSeconds = playheadSeconds, IsFinished = isFinished, CreatedBy = createdBy, HistoryId = existingHistoryId });
            }
            else
            {
                string insertQuery = @"
                    INSERT INTO tbl_WatchHistory 
                    (WatchHistoryGuid, UserId, MovieId, WatchedDate, WatchMinutes, DeviceType, PlayheadSeconds, IsFinished, ProfileGuid, IsActive, CreatedOn, CreatedBy)
                    VALUES 
                    (@Guid, @UserId, @MovieId, @WatchedDate, @WatchMinutes, @DeviceType, @PlayheadSeconds, @IsFinished, @ProfileGuid, 1, GETDATE(), @CreatedBy)";
                
                await conn.ExecuteAsync(insertQuery, new {
                    Guid = System.Guid.NewGuid().ToString("N"),
                    UserId = userId.Value,
                    MovieId = movieId.Value,
                    WatchedDate = watchedDate,
                    WatchMinutes = watchMinutes,
                    DeviceType = deviceType,
                    PlayheadSeconds = playheadSeconds,
                    IsFinished = isFinished,
                    ProfileGuid = profileGuid,
                    CreatedBy = createdBy
                });
            }
        }

        public async Task<List<object>> GetPagedAsync(string userGuid, int page, int pageSize, string? profileGuid = null)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            var offset = (page - 1) * pageSize;
            
            string query = @"
                SELECT 
                    w.WatchHistoryGuid, w.WatchedDate, w.WatchMinutes, w.DeviceType, w.PlayheadSeconds, w.IsFinished, w.ProfileGuid,
                    m.MovieGuid, m.MovieName, m.DurationMinutes, m.Language, m.ThumbnailUrl, m.ThumbnailPublicId
                FROM tbl_WatchHistory w
                INNER JOIN mst_User u ON w.UserId = u.UserId
                INNER JOIN mst_Movie m ON w.MovieId = m.MovieId
                WHERE u.UserGuid = @UserGuid AND w.IsActive = 1 " + 
                (profileGuid != null ? "AND w.ProfileGuid = @ProfileGuid " : "AND w.ProfileGuid IS NULL ") + 
                @"ORDER BY w.WatchedDate DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var rows = await conn.QueryAsync<dynamic>(query, new { UserGuid = userGuid, ProfileGuid = profileGuid, Offset = offset, PageSize = pageSize });
            return rows.Select(r => (object)r).ToList();
        }

        public async Task<object?> GetResumeProgressAsync(string userGuid, string movieGuid, string? profileGuid = null)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            string query = @"
                SELECT 
                    w.PlayheadSeconds, w.IsFinished, w.WatchedDate as LastWatchedDate,
                    m.MovieGuid, m.MovieName, m.DurationMinutes
                FROM tbl_WatchHistory w
                INNER JOIN mst_User u ON w.UserId = u.UserId
                INNER JOIN mst_Movie m ON w.MovieId = m.MovieId
                WHERE u.UserGuid = @UserGuid AND m.MovieGuid = @MovieGuid AND w.IsActive = 1 " +
                (profileGuid != null ? "AND w.ProfileGuid = @ProfileGuid " : "AND w.ProfileGuid IS NULL ");

            return await conn.QueryFirstOrDefaultAsync<dynamic>(query, new { UserGuid = userGuid, MovieGuid = movieGuid, ProfileGuid = profileGuid });
        }
    }
}

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

        public async Task InsertAsync(int userId, int movieId, System.DateTime watchedDate, int watchMinutes, string deviceType)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            await conn.ExecuteAsync(AppConstants.SpInsertWatchHistory, new { UserId = userId, MovieId = movieId, WatchedDate = watchedDate, WatchMinutes = watchMinutes, DeviceType = deviceType, CreatedBy = "system" }, commandType: CommandType.StoredProcedure);
        }

        public async Task<List<object>> GetPagedAsync(int userId, int page, int pageSize)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            var offset = (page - 1) * pageSize;
            var rows = await conn.QueryAsync<dynamic>("SELECT M.MovieName, W.WatchedDate, W.WatchMinutes, W.DeviceType FROM tbl_WatchHistory W INNER JOIN mst_Movie M ON W.MovieId = M.MovieId WHERE W.UserId = @UserId ORDER BY W.WatchedDate DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY", new { UserId = userId, Offset = offset, PageSize = pageSize }, commandType: CommandType.Text);
            return rows.Select(r => (object)r).ToList();
        }
    }
}

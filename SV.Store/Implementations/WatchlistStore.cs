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

        public async Task AddAsync(int userId, int movieId)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            await conn.ExecuteAsync(AppConstants.SpAddToWatchlist, new { UserId = userId, MovieId = movieId, CreatedBy = "system" }, commandType: CommandType.StoredProcedure);
        }

        public async Task<List<object>> GetByUserAsync(string userGuid)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            var rows = await conn.QueryAsync<dynamic>(AppConstants.SpGetUserWatchlist, new { UserGuid = userGuid }, commandType: CommandType.StoredProcedure);
            return rows.Select(r => (object)r).ToList();
        }

        public async Task RemoveAsync(string watchlistGuid, string updatedBy)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            await conn.ExecuteAsync(AppConstants.SpRemoveFromWatchlist, new { WatchlistGuid = watchlistGuid, UpdatedBy = updatedBy }, commandType: CommandType.StoredProcedure);
        }
    }
}

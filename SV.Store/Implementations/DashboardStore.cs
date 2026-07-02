using System.Threading.Tasks;
using Dapper;
using SV.Store.Abstractions;
using SV.Data.Connections;
using System.Data;
using SV.Common.Constants;

namespace SV.Store.Implementations
{
    public class DashboardStore : IDashboardStore
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public DashboardStore(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<object> GetAdminAsync()
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            // Try QueryMultiple in case SP returns multiple resultsets
            using var multi = await conn.QueryMultipleAsync(AppConstants.SpGetAdminDashboard, commandType: CommandType.StoredProcedure);
            var all = new List<object>();
            try
            {
                while (!multi.IsConsumed)
                {
                    var rows = (await multi.ReadAsync<dynamic>()).ToList();
                    foreach (var r in rows) all.Add((object)r);
                }
            }
            catch
            {
                // fallback to single query
                var rows = await conn.QueryAsync<dynamic>(AppConstants.SpGetAdminDashboard, commandType: CommandType.StoredProcedure);
                return rows.Select(r => (object)r).ToList();
            }

            return all;
        }

        public async Task<object> GetUserAsync(string userGuid)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            using var multi = await conn.QueryMultipleAsync(AppConstants.SpGetUserDashboard, new { UserGuid = userGuid }, commandType: CommandType.StoredProcedure);
            // Expecting multiple result sets: Subscriptions, RecentWatches, Watchlist, Recommendations
            var subscriptions = new List<object>();
            var recent = new List<object>();
            var watchlist = new List<object>();
            var recommendations = new List<object>();

            try
            {
                if (!multi.IsConsumed)
                {
                    var rows = (await multi.ReadAsync<dynamic>()).ToList();
                    subscriptions.AddRange(rows.Select(r => (object)r));
                }
                if (!multi.IsConsumed)
                {
                    var rows = (await multi.ReadAsync<dynamic>()).ToList();
                    recent.AddRange(rows.Select(r => (object)r));
                }
                if (!multi.IsConsumed)
                {
                    var rows = (await multi.ReadAsync<dynamic>()).ToList();
                    watchlist.AddRange(rows.Select(r => (object)r));
                }
                if (!multi.IsConsumed)
                {
                    var rows = (await multi.ReadAsync<dynamic>()).ToList();
                    recommendations.AddRange(rows.Select(r => (object)r));
                }
            }
            catch
            {
                // fallback to single result set
                var rows = await conn.QueryAsync<dynamic>(AppConstants.SpGetUserDashboard, new { UserGuid = userGuid }, commandType: CommandType.StoredProcedure);
                return rows.Select(r => (object)r).ToList();
            }

            return new
            {
                Subscriptions = subscriptions,
                RecentWatches = recent,
                Watchlist = watchlist,
                Recommendations = recommendations
            };
        }
    }
}

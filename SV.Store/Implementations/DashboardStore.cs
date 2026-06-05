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

            // Call stored procedure usp_AdminDashboard which returns multiple result sets
            using var multi = await conn.QueryMultipleAsync("usp_AdminDashboard", commandType: CommandType.StoredProcedure);
            var totalUsers = await multi.ReadFirstOrDefaultAsync<int>();
            var totalMovies = await multi.ReadFirstOrDefaultAsync<int>();
            var totalSubscriptions = await multi.ReadFirstOrDefaultAsync<int>();
            var totalRevenue = await multi.ReadFirstOrDefaultAsync<decimal>();

            return new
            {
                TotalUsers = totalUsers,
                TotalMovies = totalMovies,
                TotalSubscriptions = totalSubscriptions,
                TotalRevenue = totalRevenue
            };
        }

        public async Task<object> GetUserAsync(string userGuid)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            using var multi = await conn.QueryMultipleAsync("usp_GetUserDashboard", new { UserGuid = userGuid }, commandType: CommandType.StoredProcedure);
            var totalWatchHistory = await multi.ReadFirstOrDefaultAsync<int>();
            var totalWatchlist = await multi.ReadFirstOrDefaultAsync<int>();
            var totalWatchMinutes = await multi.ReadFirstOrDefaultAsync<int>();

            return new
            {
                TotalWatchHistory = totalWatchHistory,
                TotalWatchlist = totalWatchlist,
                TotalWatchMinutes = totalWatchMinutes
            };
        }
    }
}

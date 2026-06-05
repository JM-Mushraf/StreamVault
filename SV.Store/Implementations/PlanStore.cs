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
    public class PlanStore : IPlanStore
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public PlanStore(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<List<object>> GetPlansAsync()
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            var rows = await conn.QueryAsync<dynamic>("SELECT PlanGuid, PlanName, MonthlyPrice, ScreenLimit, VideoQuality FROM mst_Plan WHERE IsActive = 1", commandType: CommandType.Text);
            return rows.Select(r => (object)r).ToList();
        }

        public async Task CreatePlanAsync(object request)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            await conn.ExecuteAsync(AppConstants.SpInsertPlan, request, commandType: CommandType.StoredProcedure);
        }
    }
}

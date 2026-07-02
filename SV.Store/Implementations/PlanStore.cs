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

        public async Task CreatePlanAsync(SV.Common.DTOs.CreatePlanRequest request, string createdBy)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            // Map request fields to stored procedure parameters
            var parameters = new
            {
                PlanName = request.Name,
                MonthlyPrice = request.Price,
                // default to 1 screen if not provided
                ScreenLimit = 1,
                VideoQuality = "HD",
                CreatedBy = createdBy
            };

            await conn.ExecuteAsync(AppConstants.SpInsertPlan, parameters, commandType: CommandType.StoredProcedure);
        }
    }
}

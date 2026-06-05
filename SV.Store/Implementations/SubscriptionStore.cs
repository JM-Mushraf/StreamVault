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
    public class SubscriptionStore : ISubscriptionStore
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public SubscriptionStore(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<List<object>> GetSubscriptionsAsync()
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            var rows = await conn.QueryAsync<dynamic>("SELECT S.SubscriptionGuid, U.FullName, P.PlanName, S.StartDate, S.EndDate, S.PaymentStatus FROM tbl_Subscription S INNER JOIN mst_User U ON S.UserId = U.UserId INNER JOIN mst_Plan P ON S.PlanId = P.PlanId WHERE S.IsActive = 1", commandType: CommandType.Text);
            return rows.Select(r => (object)r).ToList();
        }
    }
}

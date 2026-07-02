using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using System.Linq;
using SV.Store.Abstractions;
using SV.Data.Connections;
using SV.Common.Constants;
using SV.Common.DTOs.Subscription;

namespace SV.Store.Implementations
{
    public class SubscriptionStore : ISubscriptionStore
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public SubscriptionStore(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<List<SubscriptionResponseDto>> GetActiveSubscriptionsAsync()
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            // Call stored procedure that returns FullName, PlanName, StartDate, EndDate
            var rows = await conn.QueryAsync<SubscriptionResponseDto>(SV.Common.Constants.AppConstants.SpGetActiveSubscriptions, commandType: CommandType.StoredProcedure);
            return rows.ToList();
        }

        public async Task<int> CreateSubscriptionAsync(int userId, string planGuid, System.DateTime startDate, System.DateTime endDate, string paymentStatus, string transactionReference, string createdBy)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            var parameters = new
            {
                UserId = userId,
                PlanGuid = planGuid,
                StartDate = startDate,
                EndDate = endDate,
                PaymentStatus = paymentStatus,
                TransactionReference = transactionReference,
                CreatedBy = createdBy
            };

            var id = await conn.QuerySingleAsync<int>(SV.Common.Constants.AppConstants.SpInsertSubscription, parameters, commandType: CommandType.StoredProcedure);
            return id;
        }
    }
}

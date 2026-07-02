using System.Threading.Tasks;
using Dapper;
using System.Data;
using SV.Store.Abstractions;
using SV.Data.Connections;
using SV.Common.Constants;

namespace SV.Store.Implementations
{
    public class PaymentStore : IPaymentStore
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public PaymentStore(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task InsertAsync(int subscriptionId, decimal amount, string paymentMethod, string transactionStatus, System.DateTime paidOn)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            // Resolve guid
            var guid = System.Guid.NewGuid().ToString("N");
            await conn.ExecuteAsync("INSERT INTO tbl_PaymentTransaction (PaymentGuid, SubscriptionId, Amount, PaymentMethod, TransactionStatus, PaidOn, IsActive, CreatedOn, CreatedBy) VALUES (@Guid, @SubscriptionId, @Amount, @PaymentMethod, @TransactionStatus, @PaidOn, 1, GETDATE(), 'system')", new { Guid = guid, SubscriptionId = subscriptionId, Amount = amount, PaymentMethod = paymentMethod, TransactionStatus = transactionStatus, PaidOn = paidOn });
        }

        public async Task<System.Collections.Generic.List<object>> GetHistoryAsync(string userGuid)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            string query = @"
                SELECT 
                    p.PaymentGuid, p.Amount, p.PaymentMethod, p.TransactionStatus, p.PaidOn,
                    s.SubscriptionGuid, s.StartDate, s.EndDate,
                    pl.PlanName, pl.MonthlyPrice
                FROM tbl_PaymentTransaction p
                INNER JOIN tbl_Subscription s ON p.SubscriptionId = s.SubscriptionId
                INNER JOIN mst_User u ON s.UserId = u.UserId
                INNER JOIN mst_Plan pl ON s.PlanId = pl.PlanId
                WHERE u.UserGuid = @UserGuid AND p.IsActive = 1
                ORDER BY p.PaidOn DESC";

            var rows = await conn.QueryAsync<dynamic>(query, new { UserGuid = userGuid });
            return rows.Select(r => (object)r).ToList();
        }

        public async Task<object?> GetReceiptDetailsAsync(string paymentGuid)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            string query = @"
                SELECT 
                    p.PaymentGuid, p.Amount, p.PaymentMethod, p.TransactionStatus, p.PaidOn,
                    s.SubscriptionGuid, s.StartDate, s.EndDate,
                    pl.PlanName, pl.MonthlyPrice,
                    u.UserGuid, u.FullName, u.Email
                FROM tbl_PaymentTransaction p
                INNER JOIN tbl_Subscription s ON p.SubscriptionId = s.SubscriptionId
                INNER JOIN mst_User u ON s.UserId = u.UserId
                INNER JOIN mst_Plan pl ON s.PlanId = pl.PlanId
                WHERE p.PaymentGuid = @PaymentGuid AND p.IsActive = 1";

            return await conn.QueryFirstOrDefaultAsync<dynamic>(query, new { PaymentGuid = paymentGuid });
        }
    }

}

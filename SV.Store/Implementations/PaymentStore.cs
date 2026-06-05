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

            await conn.ExecuteAsync(AppConstants.SpInsertPayment, new { SubscriptionId = subscriptionId, Amount = amount, PaymentMethod = paymentMethod, TransactionStatus = transactionStatus, PaidOn = paidOn, CreatedBy = "system" }, commandType: CommandType.StoredProcedure);
        }
    }
}

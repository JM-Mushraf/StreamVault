using System.Threading.Tasks;
using SV.Service.Abstractions;
using SV.Store.Abstractions;

namespace SV.Service.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentStore _store;

        public PaymentService(IPaymentStore store)
        {
            _store = store;
        }

        public Task InsertPaymentAsync(int subscriptionId, decimal amount, string paymentMethod, string transactionStatus, System.DateTime paidOn)
        {
            return _store.InsertAsync(subscriptionId, amount, paymentMethod, transactionStatus, paidOn);
        }

        public Task<System.Collections.Generic.List<object>> GetPaymentHistoryAsync(string userGuid)
        {
            return _store.GetHistoryAsync(userGuid);
        }

        public Task<object?> GetReceiptDetailsAsync(string paymentGuid)
        {
            return _store.GetReceiptDetailsAsync(paymentGuid);
        }
    }
}

using System.Threading.Tasks;

namespace SV.Store.Abstractions
{
    public interface IPaymentStore
    {
        Task InsertAsync(int subscriptionId, decimal amount, string paymentMethod, string transactionStatus, System.DateTime paidOn);
        Task<System.Collections.Generic.List<object>> GetHistoryAsync(string userGuid);
        Task<object?> GetReceiptDetailsAsync(string paymentGuid);
    }
}

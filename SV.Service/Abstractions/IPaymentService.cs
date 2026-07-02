using System.Threading.Tasks;

namespace SV.Service.Abstractions
{
    public interface IPaymentService
    {
        Task InsertPaymentAsync(int subscriptionId, decimal amount, string paymentMethod, string transactionStatus, System.DateTime paidOn);
        Task<System.Collections.Generic.List<object>> GetPaymentHistoryAsync(string userGuid);
        Task<object?> GetReceiptDetailsAsync(string paymentGuid);
    }
}

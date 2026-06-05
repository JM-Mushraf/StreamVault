using System.Threading.Tasks;

namespace SV.Service.Abstractions
{
    public interface IPaymentService
    {
        Task InsertPaymentAsync(int subscriptionId, decimal amount, string paymentMethod, string transactionStatus, System.DateTime paidOn);
    }
}

using System.Threading.Tasks;
using SV.Service.Abstractions;

namespace SV.Service.Implementations
{
    public class PaymentService : IPaymentService
    {
        public Task InsertPaymentAsync(int subscriptionId, decimal amount, string paymentMethod, string transactionStatus, System.DateTime paidOn)
        {
            return Task.CompletedTask;
        }
    }
}

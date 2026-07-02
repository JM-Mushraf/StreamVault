using System.Collections.Generic;
using System.Threading.Tasks;
using SV.Common.DTOs.Subscription;

namespace SV.Store.Abstractions
{
    public interface ISubscriptionStore
    {
        Task<List<SubscriptionResponseDto>> GetActiveSubscriptionsAsync();
        Task<int> CreateSubscriptionAsync(int userId, string planGuid, System.DateTime startDate, System.DateTime endDate, string paymentStatus, string transactionReference, string createdBy);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using SV.Common.DTOs.Subscription;

namespace SV.Service.Abstractions
{
    public interface ISubscriptionService
    {
        Task CreateSubscriptionAsync(int userId, string planGuid, System.DateTime startDate, System.DateTime endDate, string status, string transactionRef, string createdBy);
        Task<List<SubscriptionResponseDto>> GetActiveSubscriptionsAsync();
    }
}

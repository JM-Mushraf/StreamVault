using System.Collections.Generic;
using System.Threading.Tasks;
using SV.Service.Abstractions;
using SV.Store.Abstractions;
using SV.Common.DTOs.Subscription;

namespace SV.Service.Implementations
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionStore _subscriptionStore;

        public SubscriptionService(ISubscriptionStore subscriptionStore)
        {
            _subscriptionStore = subscriptionStore;
        }

        public Task CreateSubscriptionAsync(int userId, int planId, System.DateTime startDate, System.DateTime endDate, string status, string transactionRef)
        {
            // Creation logic is not changed here. Keep as a no-op for now.
            return Task.CompletedTask;
        }

        public Task CreateSubscriptionAsync(int userId, string planGuid, System.DateTime startDate, System.DateTime endDate, string status, string transactionRef, string createdBy)
        {
            return _subscriptionStore.CreateSubscriptionAsync(userId, planGuid, startDate, endDate, status, transactionRef, createdBy).ContinueWith(t => { });
        }

        public async Task<List<SubscriptionResponseDto>> GetActiveSubscriptionsAsync()
        {
            var subs = await _subscriptionStore.GetActiveSubscriptionsAsync();
            return subs ?? new List<SubscriptionResponseDto>();
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using SV.Service.Abstractions;

namespace SV.Service.Implementations
{
    public class SubscriptionService : ISubscriptionService
    {
        public Task CreateSubscriptionAsync(int userId, int planId, System.DateTime startDate, System.DateTime endDate, string status, string transactionRef)
        {
            return Task.CompletedTask;
        }

        public Task<List<object>> GetActiveSubscriptionsAsync()
        {
            return Task.FromResult(new List<object>());
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;

namespace SV.Service.Abstractions
{
    public interface ISubscriptionService
    {
        Task CreateSubscriptionAsync(int userId, int planId, System.DateTime startDate, System.DateTime endDate, string status, string transactionRef);
        Task<List<object>> GetActiveSubscriptionsAsync();
    }
}

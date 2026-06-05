using System.Collections.Generic;
using System.Threading.Tasks;

namespace SV.Store.Abstractions
{
    public interface ISubscriptionStore
    {
        Task<List<object>> GetSubscriptionsAsync();
    }
}

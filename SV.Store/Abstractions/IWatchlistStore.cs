using System.Collections.Generic;
using System.Threading.Tasks;

namespace SV.Store.Abstractions
{
    public interface IWatchlistStore
    {
        Task AddAsync(string userGuid, string movieGuid, string createdBy, string? profileGuid = null);
        Task RemoveAsync(string watchlistGuid, string updatedBy);
        Task<List<object>> GetByUserAsync(string userGuid, string? profileGuid = null);
    }
}

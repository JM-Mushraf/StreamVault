using System.Collections.Generic;
using System.Threading.Tasks;

namespace SV.Store.Abstractions
{
    public interface IWatchlistStore
    {
        Task AddAsync(int userId, int movieId);
        Task RemoveAsync(string watchlistGuid, string updatedBy);
        Task<List<object>> GetByUserAsync(string userGuid);
    }
}

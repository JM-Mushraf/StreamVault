using System.Collections.Generic;
using System.Threading.Tasks;

namespace SV.Service.Abstractions
{
    public interface IWatchlistService
    {
        Task AddToWatchlistAsync(int userId, int movieId);
        Task RemoveFromWatchlistAsync(string watchlistGuid, string updatedBy);
        Task<List<object>> GetUserWatchlistAsync(string userGuid);
    }
}

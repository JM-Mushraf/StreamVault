using System.Collections.Generic;
using System.Threading.Tasks;
using SV.Service.Abstractions;

namespace SV.Service.Implementations
{
    public class WatchlistService : IWatchlistService
    {
        public Task AddToWatchlistAsync(int userId, int movieId)
        {
            return Task.CompletedTask;
        }

        public Task RemoveFromWatchlistAsync(string watchlistGuid, string updatedBy)
        {
            return Task.CompletedTask;
        }

        public Task<List<object>> GetUserWatchlistAsync(string userGuid)
        {
            return Task.FromResult(new List<object>());
        }
    }
}

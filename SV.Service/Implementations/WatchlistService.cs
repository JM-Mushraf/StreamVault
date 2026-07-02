using System.Collections.Generic;
using System.Threading.Tasks;
using SV.Service.Abstractions;

namespace SV.Service.Implementations
{
    public class WatchlistService : IWatchlistService
    {
        private readonly SV.Store.Abstractions.IWatchlistStore _store;

        public WatchlistService(SV.Store.Abstractions.IWatchlistStore store)
        {
            _store = store;
        }
        public Task AddToWatchlistAsync(string userGuid, string movieGuid, string createdBy, string? profileGuid = null)
        {
            return _store.AddAsync(userGuid, movieGuid, createdBy, profileGuid);
        }

        public Task RemoveFromWatchlistAsync(string watchlistGuid, string updatedBy)
        {
            return _store.RemoveAsync(watchlistGuid, updatedBy);
        }

        public Task<List<object>> GetUserWatchlistAsync(string userGuid, string? profileGuid = null)
        {
            return _store.GetByUserAsync(userGuid, profileGuid);
        }
    }
}

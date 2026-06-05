using System.Collections.Generic;
using System.Threading.Tasks;
using SV.Service.Abstractions;

namespace SV.Service.Implementations
{
    public class WatchHistoryService : IWatchHistoryService
    {
        public Task InsertWatchHistoryAsync(int userId, int movieId, System.DateTime watchedDate, int watchMinutes, string deviceType)
        {
            return Task.CompletedTask;
        }

        public Task<List<object>> GetWatchHistoryPagedAsync(int userId, int page, int pageSize)
        {
            return Task.FromResult(new List<object>());
        }
    }
}

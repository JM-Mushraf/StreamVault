using System.Collections.Generic;
using System.Threading.Tasks;
using SV.Service.Abstractions;

namespace SV.Service.Implementations
{
    public class WatchHistoryService : IWatchHistoryService
    {
        private readonly SV.Store.Abstractions.IWatchHistoryStore _store;

        public WatchHistoryService(SV.Store.Abstractions.IWatchHistoryStore store)
        {
            _store = store;
        }
        public Task InsertWatchHistoryAsync(string userGuid, string movieGuid, System.DateTime watchedDate, int watchMinutes, string deviceType, string createdBy, int playheadSeconds = 0, bool isFinished = false, string? profileGuid = null)
        {
            return _store.InsertAsync(userGuid, movieGuid, watchedDate, watchMinutes, deviceType, createdBy, playheadSeconds, isFinished, profileGuid);
        }

        public Task<List<object>> GetWatchHistoryPagedAsync(string userGuid, int page, int pageSize, string? profileGuid = null)
        {
            return _store.GetPagedAsync(userGuid, page, pageSize, profileGuid);
        }

        public Task<object?> GetResumeProgressAsync(string userGuid, string movieGuid, string? profileGuid = null)
        {
            return _store.GetResumeProgressAsync(userGuid, movieGuid, profileGuid);
        }
    }
}

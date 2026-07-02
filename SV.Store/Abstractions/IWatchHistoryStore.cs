using System.Collections.Generic;
using System.Threading.Tasks;

namespace SV.Store.Abstractions
{
    public interface IWatchHistoryStore
    {
        Task InsertAsync(string userGuid, string movieGuid, System.DateTime watchedDate, int watchMinutes, string deviceType, string createdBy, int playheadSeconds = 0, bool isFinished = false, string? profileGuid = null);
        Task<List<object>> GetPagedAsync(string userGuid, int page, int pageSize, string? profileGuid = null);
        Task<object?> GetResumeProgressAsync(string userGuid, string movieGuid, string? profileGuid = null);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;

namespace SV.Service.Abstractions
{
    public interface IWatchHistoryService
    {
        Task InsertWatchHistoryAsync(string userGuid, string movieGuid, System.DateTime watchedDate, int watchMinutes, string deviceType, string createdBy, int playheadSeconds = 0, bool isFinished = false, string? profileGuid = null);
        Task<List<object>> GetWatchHistoryPagedAsync(string userGuid, int page, int pageSize, string? profileGuid = null);
        Task<object?> GetResumeProgressAsync(string userGuid, string movieGuid, string? profileGuid = null);
    }
}

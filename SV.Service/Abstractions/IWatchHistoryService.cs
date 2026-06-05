using System.Collections.Generic;
using System.Threading.Tasks;

namespace SV.Service.Abstractions
{
    public interface IWatchHistoryService
    {
        Task InsertWatchHistoryAsync(int userId, int movieId, System.DateTime watchedDate, int watchMinutes, string deviceType);
        Task<List<object>> GetWatchHistoryPagedAsync(int userId, int page, int pageSize);
    }
}

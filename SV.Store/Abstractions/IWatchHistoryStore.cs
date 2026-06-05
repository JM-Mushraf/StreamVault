using System.Collections.Generic;
using System.Threading.Tasks;

namespace SV.Store.Abstractions
{
    public interface IWatchHistoryStore
    {
        Task InsertAsync(int userId, int movieId, System.DateTime watchedDate, int watchMinutes, string deviceType);
        Task<List<object>> GetPagedAsync(int userId, int page, int pageSize);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;

namespace SV.Service.Abstractions
{
    public interface IWatchlistService
    {
        Task AddToWatchlistAsync(string userGuid, string movieGuid, string createdBy, string? profileGuid = null);
        Task RemoveFromWatchlistAsync(string watchlistGuid, string updatedBy);
        Task<List<object>> GetUserWatchlistAsync(string userGuid, string? profileGuid = null);
    }
}

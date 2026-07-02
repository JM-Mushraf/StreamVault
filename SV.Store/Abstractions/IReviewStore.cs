using System.Collections.Generic;
using System.Threading.Tasks;

namespace SV.Store.Abstractions
{
    public interface IReviewStore
    {
        Task AddAsync(string userGuid, string movieGuid, int rating, string? reviewText, string createdBy, string? profileGuid = null);
        Task<List<object>> GetByMovieAsync(string movieGuid);
    }
}

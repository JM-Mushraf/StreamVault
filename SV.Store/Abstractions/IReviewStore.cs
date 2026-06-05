using System.Collections.Generic;
using System.Threading.Tasks;

namespace SV.Store.Abstractions
{
    public interface IReviewStore
    {
        Task AddAsync(int userId, int movieId, decimal rating, string? reviewText);
        Task<List<object>> GetByMovieAsync(string movieGuid);
    }
}

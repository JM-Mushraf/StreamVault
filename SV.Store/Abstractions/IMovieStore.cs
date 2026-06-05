using System.Collections.Generic;
using System.Threading.Tasks;

namespace SV.Store.Abstractions
{
    public interface IMovieStore
    {
        Task<List<object>> GetLatestMoviesAsync();
        Task CreateMovieAsync(SV.Common.DTOs.CreateMovieRequest request);
    }
}

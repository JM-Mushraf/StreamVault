using System.Collections.Generic;
using System.Threading.Tasks;

namespace SV.Service.Abstractions
{
    public interface IMovieService
    {
        Task<List<object>> GetLatestMoviesAsync();
        Task CreateMovieAsync(SV.Common.DTOs.CreateMovieRequest request);
    }
}

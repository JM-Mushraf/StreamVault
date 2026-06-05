using System.Collections.Generic;
using System.Threading.Tasks;
using SV.Service.Abstractions;

namespace SV.Service.Implementations
{
    public class MovieService : IMovieService
    {
        public Task<List<object>> GetLatestMoviesAsync()
        {
            return Task.FromResult(new List<object>());
        }

        public Task CreateMovieAsync(SV.Common.DTOs.CreateMovieRequest request)
        {
            return Task.CompletedTask;
        }
    }
}

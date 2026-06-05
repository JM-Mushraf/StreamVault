using System.Collections.Generic;
using System.Threading.Tasks;
using SV.Service.Abstractions;

namespace SV.Service.Implementations
{
    public class GenreService : IGenreService
    {
        private readonly SV.Store.Abstractions.IGenreStore _genreStore;

        public GenreService(SV.Store.Abstractions.IGenreStore genreStore)
        {
            _genreStore = genreStore;
        }

        public Task<List<object>> GetGenresAsync()
        {
            return _genreStore.GetGenresAsync();
        }

        public async Task CreateGenreAsync(string name, string createdBy)
        {
            await _genreStore.CreateGenreAsync(name, createdBy);
        }
    }
}

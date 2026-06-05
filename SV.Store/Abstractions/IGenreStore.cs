using System.Collections.Generic;
using System.Threading.Tasks;

namespace SV.Store.Abstractions
{
    public interface IGenreStore
    {
        Task<List<object>> GetGenresAsync();
        Task<int> CreateGenreAsync(string name, string createdBy);
    }
}

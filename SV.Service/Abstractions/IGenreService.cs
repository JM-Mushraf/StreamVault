
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SV.Service.Abstractions
{
    public interface IGenreService
    {
        Task<List<object>> GetGenresAsync();
        Task CreateGenreAsync(string name, string createdBy);
    }
}


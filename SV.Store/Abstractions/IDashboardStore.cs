using System.Threading.Tasks;

namespace SV.Store.Abstractions
{
    public interface IDashboardStore
    {
        Task<object> GetAdminAsync();
        Task<object> GetUserAsync(string userGuid);
    }
}

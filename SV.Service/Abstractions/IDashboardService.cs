using System.Threading.Tasks;

namespace SV.Service.Abstractions
{
    public interface IDashboardService
    {
        Task<object> GetAdminDashboardAsync();
        Task<object> GetUserDashboardAsync(string userGuid);
    }
}

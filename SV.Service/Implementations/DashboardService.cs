using System.Threading.Tasks;
using SV.Service.Abstractions;

namespace SV.Service.Implementations
{
    public class DashboardService : IDashboardService
    {
        public Task<object> GetAdminDashboardAsync()
        {
            return Task.FromResult<object>(new { });
        }

        public Task<object> GetUserDashboardAsync(string userGuid)
        {
            return Task.FromResult<object>(new { });
        }
    }
}

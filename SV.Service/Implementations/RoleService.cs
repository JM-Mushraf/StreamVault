using System.Threading.Tasks;
using SV.Service.Abstractions;

namespace SV.Service.Implementations
{
    public class RoleService : IRoleService
    {
        public Task CreateRoleAsync(string roleName, string createdBy)
        {
            return Task.CompletedTask;
        }

        public Task UpdateRoleAsync(string roleGuid, string roleName, string updatedBy)
        {
            return Task.CompletedTask;
        }

        public Task DeleteRoleAsync(string roleGuid, string updatedBy)
        {
            return Task.CompletedTask;
        }
    }
}

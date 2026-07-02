using System.Threading.Tasks;

namespace SV.Service.Abstractions
{
    public interface IRoleService
    {
        Task<string> CreateRoleAsync(string roleName, string createdBy);
        Task UpdateRoleAsync(string roleGuid, string roleName, string updatedBy);
        Task DeleteRoleAsync(string roleGuid, string updatedBy);
        Task<System.Collections.Generic.List<object>> GetActiveRolesAsync();
    }
}

using System.Threading.Tasks;

namespace SV.Service.Abstractions
{
    public interface IRoleService
    {
        Task CreateRoleAsync(string roleName, string createdBy);
        Task UpdateRoleAsync(string roleGuid, string roleName, string updatedBy);
        Task DeleteRoleAsync(string roleGuid, string updatedBy);
    }
}

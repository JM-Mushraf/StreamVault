using System.Threading.Tasks;

namespace SV.Store.Abstractions
{
    public interface IRoleStore
    {
        Task<string> CreateAsync(string roleName, string createdBy);
        Task UpdateAsync(string roleGuid, string roleName, string updatedBy);
        Task DeleteAsync(string roleGuid, string updatedBy);
        Task<bool> ExistsByNameAsync(string roleName);
        Task<System.Collections.Generic.List<object>> GetActiveAsync();
    }
}

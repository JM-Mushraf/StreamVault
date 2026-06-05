using System.Threading.Tasks;

namespace SV.Store.Abstractions
{
    public interface IRoleStore
    {
        Task CreateAsync(string roleName, string createdBy);
        Task UpdateAsync(string roleGuid, string roleName, string updatedBy);
        Task DeleteAsync(string roleGuid, string updatedBy);
    }
}

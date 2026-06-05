using System.Threading.Tasks;
using SV.Store.Abstractions;

namespace SV.Store.Implementations
{
    public class RoleStore : IRoleStore
    {
        public Task CreateAsync(string roleName, string createdBy) => Task.CompletedTask;
        public Task UpdateAsync(string roleGuid, string roleName, string updatedBy) => Task.CompletedTask;
        public Task DeleteAsync(string roleGuid, string updatedBy) => Task.CompletedTask;
    }
}

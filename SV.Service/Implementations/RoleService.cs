using System.Threading.Tasks;
using SV.Service.Abstractions;

namespace SV.Service.Implementations
{
    public class RoleService : IRoleService
    {
        private readonly SV.Store.Abstractions.IRoleStore _store;

        public RoleService(SV.Store.Abstractions.IRoleStore store)
        {
            _store = store;
        }

        public async Task<string> CreateRoleAsync(string roleName, string createdBy)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                throw new System.ArgumentException("roleName is required", nameof(roleName));

            var exists = await _store.ExistsByNameAsync(roleName);
            if (exists) throw new System.InvalidOperationException("Role name already exists");

            return await _store.CreateAsync(roleName, createdBy);
        }

        public Task UpdateRoleAsync(string roleGuid, string roleName, string updatedBy)
        {
            return _store.UpdateAsync(roleGuid, roleName, updatedBy);
        }

        public Task DeleteRoleAsync(string roleGuid, string updatedBy)
        {
            return _store.DeleteAsync(roleGuid, updatedBy);
        }

        public Task<System.Collections.Generic.List<object>> GetActiveRolesAsync()
        {
            return _store.GetActiveAsync();
        }
    }
}

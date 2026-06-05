using System.Collections.Generic;
using System.Threading.Tasks;
using SV.Service.Abstractions;
using SV.Store.Abstractions;

namespace SV.Service.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserStore _userStore;

        public UserService(IUserStore userStore)
        {
            _userStore = userStore;
        }

        public Task<string> CreateUserAsync(object request, string roleName = "User") => Task.FromResult(string.Empty);

        public Task<bool> DeleteUserAsync(string userGuid) => Task.FromResult(true);

        public Task<object?> GetUserByIdAsync(string userGuid) => Task.FromResult<object?>(null);
        public Task<object?> GetUserByGuidAsync(string userGuid) => Task.FromResult<object?>(null);

        public Task<object> GetPagedUsersAsync(int pageNumber, int pageSize) => Task.FromResult<object>(new { });

        public Task<List<object>> GetAllUsersAsync() => Task.FromResult(new List<object>());

        public Task<bool> UpdateUserAsync(string userGuid, object request) => Task.FromResult(true);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;

namespace SV.Service.Abstractions
{
    public interface IUserService
    {
        Task<List<object>> GetAllUsersAsync();
        Task<object?> GetUserByIdAsync(string userGuid);
        Task<object?> GetUserByGuidAsync(string userGuid);
        Task<string> CreateUserAsync(object request, string roleName = "User");
        Task<bool> UpdateUserAsync(string userGuid, object request);
        Task<bool> DeleteUserAsync(string userGuid);
        Task<object> GetPagedUsersAsync(int pageNumber, int pageSize);
    }
}

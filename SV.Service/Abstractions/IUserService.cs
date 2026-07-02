using System.Collections.Generic;
using System.Threading.Tasks;
using SV.Service.Abstractions;

namespace SV.Service.Abstractions
{
    public interface IUserService
    {
        Task<List<object>> GetAllUsersAsync();
        Task<object?> GetUserByIdAsync(string userGuid);
        Task<object?> GetUserByGuidAsync(string userGuid);
        Task<object?> UploadAvatarAsync(string userGuid, SV.Common.DTOs.FileUploadDto file);
        Task<string> CreateUserAsync(object request, string roleName = "User");
        Task<bool> UpdateUserAsync(
            string userGuid, 
            SV.Common.DTOs.Auth.UpdateUserRequest request, 
            SV.Common.DTOs.FileUploadDto? profilePicture = null);
        Task<bool> DeleteUserAsync(string userGuid);
        Task<object> GetPagedUsersAsync(int pageNumber, int pageSize);
    }
}

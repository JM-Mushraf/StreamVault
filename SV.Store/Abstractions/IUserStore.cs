using System.Threading.Tasks;
using SV.Common.DTOs;

namespace SV.Store.Abstractions
{
    public interface IUserStore
    {
        Task<int?> GetUserIdByGuidAsync(string userGuid);
        Task<string?> GetUserGuidByIdAsync(int userId);
        Task<bool> UpdateUserAvatarAsync(string userGuid, string avatarUrl, string avatarPublicId);
        Task<UserResponseDto?> GetUserByGuidAsync(string userGuid);
        Task<bool> UpdateUserAsync(
            string userGuid,
            string? fullName,
            string? mobile,
            string? country,
            string updatedBy,
            string? userProfileImageUrl,
            string? userProfileImagePublicId);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using SV.Service.Abstractions;
using SV.Store.Abstractions;

namespace SV.Service.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserStore _userStore;
        private readonly SV.Common.Abstractions.ICloudinaryService _cloudinaryService;

        public UserService(IUserStore userStore, SV.Common.Abstractions.ICloudinaryService cloudinaryService)
        {
            _userStore = userStore;
            _cloudinaryService = cloudinaryService;
        }

        public Task<string> CreateUserAsync(object request, string roleName = "User") => Task.FromResult(string.Empty);

        public Task<bool> DeleteUserAsync(string userGuid) => Task.FromResult(true);

        public Task<object?> GetUserByIdAsync(string userGuid) => Task.FromResult<object?>(null);
        public async Task<object?> GetUserByGuidAsync(string userGuid)
        {
            return await _userStore.GetUserByGuidAsync(userGuid);
        }

        public Task<object> GetPagedUsersAsync(int pageNumber, int pageSize) => Task.FromResult<object>(new { });

        public Task<List<object>> GetAllUsersAsync() => Task.FromResult(new List<object>());

        public async Task<bool> UpdateUserAsync(
            string userGuid, 
            SV.Common.DTOs.Auth.UpdateUserRequest request, 
            SV.Common.DTOs.FileUploadDto? profilePicture = null)
        {
            // 1. Fetch existing user details
            var existingUser = await _userStore.GetUserByGuidAsync(userGuid);
            if (existingUser == null)
            {
                return false;
            }

            string? profileImageUrl = existingUser.UserProfileImageUrl;
            string? profileImagePublicId = existingUser.UserProfileImagePublicId;

            // 2. Handle new profile picture upload
            if (profilePicture != null)
            {
                var result = await _cloudinaryService.UploadImageAsync(profilePicture, "streamvault/users");
                profileImageUrl = result.SecureUrl;
                profileImagePublicId = result.PublicId;

                // Delete old profile image from Cloudinary to clean up
                if (!string.IsNullOrWhiteSpace(existingUser.UserProfileImagePublicId))
                {
                    await _cloudinaryService.DeleteResourceAsync(existingUser.UserProfileImagePublicId, "image");
                }
            }

            // 3. Update in database
            return await _userStore.UpdateUserAsync(
                userGuid,
                request.FullName,
                request.Mobile,
                request.Country,
                request.FullName ?? existingUser.FullName ?? "system",
                profileImageUrl,
                profileImagePublicId
            );
        }

        public async Task<object?> UploadAvatarAsync(string userGuid, SV.Common.DTOs.FileUploadDto file)
        {
            if (file == null) return null;

            var result = await _cloudinaryService.UploadImageAsync(file, "streamvault/users");

            // persist to DB via store
            var persisted = await _userStore.UpdateUserAvatarAsync(userGuid, result.SecureUrl, result.PublicId);
            if (!persisted) return null;

            var user = await _userStore.GetUserByGuidAsync(userGuid);
            return user;
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using SV.Common.DTOs.Profile;
using SV.Common.Models;
using SV.Service.Abstractions;
using SV.Store.Abstractions;

namespace SV.Service.Implementations
{
    public class ProfileService : IProfileService
    {
        private readonly IProfileStore _profileStore;

        public ProfileService(IProfileStore profileStore)
        {
            _profileStore = profileStore;
        }

        public async Task<ApiResponse<List<ProfileResponseDto>>> GetProfilesAsync(string userGuid)
        {
            var list = await _profileStore.GetByAccountAsync(userGuid);
            return new ApiResponse<List<ProfileResponseDto>>
            {
                Success = true,
                Message = "Profiles retrieved successfully.",
                Data = list
            };
        }

        public async Task<ApiResponse<string>> CreateProfileAsync(string userGuid, CreateProfileDto dto, string createdBy)
        {
            var currentCount = await _profileStore.GetCountByAccountAsync(userGuid);
            if (currentCount >= 5)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Profiles limit reached. You can have a maximum of 5 profiles per account.",
                    Data = string.Empty
                };
            }

            if (string.IsNullOrWhiteSpace(dto.ProfileName))
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Profile name is required.",
                    Data = string.Empty
                };
            }

            var guid = await _profileStore.CreateAsync(userGuid, dto, createdBy);
            if (guid == null)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Failed to create profile.",
                    Data = string.Empty
                };
            }

            return new ApiResponse<string>
            {
                Success = true,
                Message = "Profile created successfully.",
                Data = guid
            };
        }

        public async Task<ApiResponse<bool>> UpdateProfileAsync(string profileGuid, CreateProfileDto dto, string updatedBy)
        {
            if (string.IsNullOrWhiteSpace(dto.ProfileName))
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Profile name is required.",
                    Data = false
                };
            }

            var success = await _profileStore.UpdateAsync(profileGuid, dto, updatedBy);
            return new ApiResponse<bool>
            {
                Success = success,
                Message = success ? "Profile updated successfully." : "Profile not found or failed to update.",
                Data = success
            };
        }

        public async Task<ApiResponse<bool>> DeleteProfileAsync(string profileGuid, string updatedBy)
        {
            var success = await _profileStore.DeleteAsync(profileGuid, updatedBy);
            return new ApiResponse<bool>
            {
                Success = success,
                Message = success ? "Profile deleted successfully." : "Profile not found or failed to delete.",
                Data = success
            };
        }
    }
}

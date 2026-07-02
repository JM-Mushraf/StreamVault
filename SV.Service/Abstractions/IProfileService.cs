using System.Collections.Generic;
using System.Threading.Tasks;
using SV.Common.DTOs.Profile;
using SV.Common.Models;

namespace SV.Service.Abstractions
{
    public interface IProfileService
    {
        Task<ApiResponse<List<ProfileResponseDto>>> GetProfilesAsync(string userGuid);
        Task<ApiResponse<string>> CreateProfileAsync(string userGuid, CreateProfileDto dto, string createdBy);
        Task<ApiResponse<bool>> UpdateProfileAsync(string profileGuid, CreateProfileDto dto, string updatedBy);
        Task<ApiResponse<bool>> DeleteProfileAsync(string profileGuid, string updatedBy);
    }
}

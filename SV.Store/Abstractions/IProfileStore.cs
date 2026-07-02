using System.Collections.Generic;
using System.Threading.Tasks;
using SV.Common.DTOs.Profile;

namespace SV.Store.Abstractions
{
    public interface IProfileStore
    {
        Task<List<ProfileResponseDto>> GetByAccountAsync(string userGuid);
        Task<string?> CreateAsync(string userGuid, CreateProfileDto dto, string createdBy);
        Task<bool> UpdateAsync(string profileGuid, CreateProfileDto dto, string updatedBy);
        Task<bool> DeleteAsync(string profileGuid, string updatedBy);
        Task<int> GetCountByAccountAsync(string userGuid);
        Task<ProfileResponseDto?> GetByGuidAsync(string profileGuid);
    }
}

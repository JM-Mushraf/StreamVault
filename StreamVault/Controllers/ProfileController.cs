using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV.Common.DTOs.Profile;
using SV.Service.Abstractions;

namespace SV.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfiles()
        {
            var userGuid = User.FindFirst("UserGuid")?.Value;
            if (string.IsNullOrEmpty(userGuid)) return Unauthorized();

            var resp = await _profileService.GetProfilesAsync(userGuid);
            return Ok(resp);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProfile([FromBody] CreateProfileDto dto)
        {
            var userGuid = User.FindFirst("UserGuid")?.Value;
            if (string.IsNullOrEmpty(userGuid)) return Unauthorized();

            var createdBy = User.Identity?.Name ?? User.FindFirst("FullName")?.Value ?? "system";
            var resp = await _profileService.CreateProfileAsync(userGuid, dto, createdBy);
            
            if (!resp.Success)
            {
                return BadRequest(resp);
            }
            return Ok(resp);
        }

        [HttpPut("{profileGuid}")]
        public async Task<IActionResult> UpdateProfile(string profileGuid, [FromBody] CreateProfileDto dto)
        {
            var updatedBy = User.Identity?.Name ?? User.FindFirst("FullName")?.Value ?? "system";
            var resp = await _profileService.UpdateProfileAsync(profileGuid, dto, updatedBy);

            if (!resp.Success)
            {
                return BadRequest(resp);
            }
            return Ok(resp);
        }

        [HttpDelete("{profileGuid}")]
        public async Task<IActionResult> DeleteProfile(string profileGuid)
        {
            var updatedBy = User.Identity?.Name ?? User.FindFirst("FullName")?.Value ?? "system";
            var resp = await _profileService.DeleteProfileAsync(profileGuid, updatedBy);

            if (!resp.Success)
            {
                return BadRequest(resp);
            }
            return Ok(resp);
        }
    }
}

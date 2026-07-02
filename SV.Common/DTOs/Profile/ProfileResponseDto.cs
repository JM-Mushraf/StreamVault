using System;

namespace SV.Common.DTOs.Profile
{
    public class ProfileResponseDto
    {
        public string ProfileGuid { get; set; } = string.Empty;
        public string UserGuid { get; set; } = string.Empty;
        public string ProfileName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public bool IsKids { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}

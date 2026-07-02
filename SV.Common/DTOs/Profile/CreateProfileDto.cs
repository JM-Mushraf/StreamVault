namespace SV.Common.DTOs.Profile
{
    public class CreateProfileDto
    {
        public string ProfileName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public bool IsKids { get; set; }
    }
}

namespace SV.Common.DTOs.Auth;

public class RegisterRequestDto
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string Mobile { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    // Optional profile picture URL (set by controller after Cloudinary upload)
    // These should be nullable and not default to empty string
    public string? ProfileImageUrl { get; set; } = null;

    // Optional profile picture public ID from Cloudinary
    public string? ProfileImagePublicId { get; set; } = null;
}
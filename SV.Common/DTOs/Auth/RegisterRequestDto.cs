namespace SV.Common.DTOs.Auth;

public class RegisterRequestDto
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string Mobile { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;
}
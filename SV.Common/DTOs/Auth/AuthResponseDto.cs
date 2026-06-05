namespace SV.Common.DTOs.Auth;

public class AuthResponseDto
{
    public string UserGuid { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;

    // RoleId from database (string to cover different schema types)
    public string RoleId { get; set; } = string.Empty;
}
using SV.Common.DTOs.Auth;

namespace SV.Store.Abstractions;

public interface IAuthStore
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);

    Task<AuthResponseDto?> LoginAsync(string email, string password);

    Task<bool> SetPasswordResetTokenAsync(string email, string token, System.DateTime expiry);

    Task<bool> UpdatePasswordAndClearTokenAsync(string token, string passwordHash);

    Task<dynamic?> GetUserByResetTokenAsync(string token);
}
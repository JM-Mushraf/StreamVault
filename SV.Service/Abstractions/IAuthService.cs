using SV.Common.DTOs.Auth;
using SV.Common.Models;

namespace SV.Service.Abstractions;

public interface IAuthService
{
    Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto request);

    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request);

    Task<ApiResponse<bool>> ForgotPasswordAsync(string email);

    Task<ApiResponse<bool>> ResetPasswordAsync(string token, string newPassword);
}
using SV.Common.DTOs.Auth;
using SV.Common.Models;
using SV.Common.Utilities;
using SV.Service.Abstractions;
using SV.Store.Abstractions;
using SV.Common.Abstractions;

namespace SV.Service.Implementations;

public class AuthService : IAuthService
{
    private readonly IAuthStore _authStore;
    private readonly JwtHelper _jwtHelper;
    private readonly IEmailService _emailService;

    public AuthService(IAuthStore authStore, JwtHelper jwtHelper, IEmailService emailService)
    {
        _authStore = authStore;
        _jwtHelper = jwtHelper;
        _emailService = emailService;
    }

    public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto request)
    {
        // Password hashing is handled inside the store layer when persisting the user.
        var user = await _authStore.RegisterAsync(request);

        var token = _jwtHelper.GenerateToken(user.UserGuid, user.Email, user.RoleId);

        return new ApiResponse<AuthResponseDto>
        {
            Success = true,
            Message = "User registered successfully",
                Data = new AuthResponseDto
                {
                    UserGuid = user.UserGuid,
                    FullName = user.FullName,
                    Email = user.Email,
                    Token = token,
                    RoleId = user.RoleId
                }
        };
    }

    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request)
    {
        var user = await _authStore.LoginAsync(request.Email, request.Password);

        if (user == null)
        {
            return new ApiResponse<AuthResponseDto>
            {
                Success = false,
                Message = "Invalid credentials"
            };
        }

        user.Token = _jwtHelper.GenerateToken(user.UserGuid, user.Email, user.RoleId);

        return new ApiResponse<AuthResponseDto>
        {
            Success = true,
            Message = "Login successful",
            Data = user
        };
    }

    public async Task<ApiResponse<bool>> ForgotPasswordAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Email is required.",
                Data = false
            };
        }

        var token = Guid.NewGuid().ToString("N");
        var expiry = DateTime.UtcNow.AddHours(24);
        var tokenSaved = await _authStore.SetPasswordResetTokenAsync(email, token, expiry);

        if (!tokenSaved)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Email address not found.",
                Data = false
            };
        }

        var resetLink = $"http://localhost:3000/reset-password?token={token}";
        var subject = "StreamVault - Reset Your Password";
        var body = $@"
            <div style='font-family: sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #eee; border-radius: 8px;'>
                <h2 style='color: #e50914;'>StreamVault</h2>
                <p>Hello,</p>
                <p>We received a request to reset your password. Please click the button below to choose a new password:</p>
                <p style='text-align: center; margin: 30px 0;'>
                    <a href='{resetLink}' style='background-color: #e50914; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; font-weight: bold; display: inline-block;'>Reset Password</a>
                </p>
                <p>This link is valid for 24 hours. If you did not request a password reset, you can safely ignore this email.</p>
                <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
                <p style='font-size: 12px; color: #777;'>StreamVault Premium Streaming Platform</p>
            </div>";

        try
        {
            await _emailService.SendEmailAsync(email, subject, body);
            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Password reset link sent to your email.",
                Data = true
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Failed to send email: {ex.Message}",
                Data = false
            };
        }
    }

    public async Task<ApiResponse<bool>> ResetPasswordAsync(string token, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(newPassword))
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Token and password are required.",
                Data = false
            };
        }

        var user = await _authStore.GetUserByResetTokenAsync(token);
        if (user == null)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Invalid reset token.",
                Data = false
            };
        }

        DateTime expiry = user.PasswordResetExpiry;
        if (expiry < DateTime.UtcNow)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Reset token has expired.",
                Data = false
            };
        }

        var passwordHash = PasswordHelper.HashPassword(newPassword);
        var updated = await _authStore.UpdatePasswordAndClearTokenAsync(token, passwordHash);

        if (!updated)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to update password.",
                Data = false
            };
        }

        return new ApiResponse<bool>
        {
            Success = true,
            Message = "Password has been successfully reset.",
            Data = true
        };
    }
}
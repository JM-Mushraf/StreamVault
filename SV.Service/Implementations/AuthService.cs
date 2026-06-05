using SV.Common.DTOs.Auth;
using SV.Common.Models;
using SV.Common.Utilities;
using SV.Service.Abstractions;
using SV.Store.Abstractions;

namespace SV.Service.Implementations;

public class AuthService : IAuthService
{
    private readonly IAuthStore _authStore;
    private readonly JwtHelper _jwtHelper;

    public AuthService(IAuthStore authStore, JwtHelper jwtHelper)
    {
        _authStore = authStore;
        _jwtHelper = jwtHelper;
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
}
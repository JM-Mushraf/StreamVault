using SV.Common.DTOs.Auth;

namespace SV.Store.Abstractions;

public interface IAuthStore
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);

    Task<AuthResponseDto?> LoginAsync(string email, string password);
}
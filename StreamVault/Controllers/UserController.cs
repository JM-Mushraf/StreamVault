using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV.Common.DTOs;
using SV.Service.Abstractions;

namespace ProjectFileStructure.Controllers;

[ApiController]
[Route("api/[controller]")]
/// <summary>
/// User controller: endpoints for user management, registration and current user info.
/// </summary>
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userGuid = User.FindFirst("UserGuid")?.Value;
        if (string.IsNullOrWhiteSpace(userGuid)) return Unauthorized();

        var user = await _userService.GetUserByGuidAsync(userGuid);
        return Ok(user);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CreateUserDto request)
    {
        // hash password before passing to service to match service expectations
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            request.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
        }

        await _userService.CreateUserAsync(request);
        return Ok(new { success = true });
    }
}

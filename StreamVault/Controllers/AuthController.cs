using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SV.Service.Abstractions;
using SV.Common.DTOs.Auth;

namespace SV.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    /// <summary>
    /// Authentication endpoints: login and registration.
    /// </summary>
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resp = await _authService.LoginAsync(request);
            if (resp == null || !resp.Success) return Unauthorized();

            var token = resp.Data?.Token ?? string.Empty;
            if (!string.IsNullOrEmpty(token))
            {
                Response.Cookies.Append("AuthToken", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddHours(1)
                });
            }

            return Ok(resp);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resp = await _authService.RegisterAsync(request);
            if (resp == null) return BadRequest();
            return Ok(resp);
        }
    }
}


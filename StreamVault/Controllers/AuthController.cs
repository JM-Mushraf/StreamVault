using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SV.Service.Abstractions;
using SV.Common.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;

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
        private readonly SV.Common.Abstractions.ICloudinaryService _cloudinaryService;
        private readonly IUserService _userService;

        public AuthController(IAuthService authService, SV.Common.Abstractions.ICloudinaryService cloudinaryService, IUserService userService)
        {
            _authService = authService;
            _cloudinaryService = cloudinaryService;
            _userService = userService;
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

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Append("AuthToken", "", new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(-1)
            });
            return Ok(new { success = true, message = "Logged out successfully" });
        }

        [HttpPost("register")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Register()
        {
            try
            {
                var form = Request.Form;

                // Parse form fields
                var fullName = form["FullName"].ToString();
                var email = form["Email"].ToString();
                var password = form["Password"].ToString();
                var mobile = form["Mobile"].ToString();
                var country = form["Country"].ToString();

                // Validate required fields
                if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                    return BadRequest("FullName, Email, and Password are required");

                // Create request DTO
                var request = new RegisterRequestDto
                {
                    FullName = fullName,
                    Email = email,
                    Password = password,
                    Mobile = string.IsNullOrWhiteSpace(mobile) ? string.Empty : mobile,
                    Country = string.IsNullOrWhiteSpace(country) ? string.Empty : country
                };

                System.Diagnostics.Debug.WriteLine($"[AUTH REGISTER] Starting registration for {email}");
                System.Diagnostics.Debug.WriteLine($"[AUTH REGISTER] Form files count: {form.Files.Count}");

                // Handle optional profile picture - try multiple possible field names
                IFormFile profilePictureFile = null;
                if (form.Files.Count > 0)
                {
                    profilePictureFile = form.Files["ProfilePicture"] ?? form.Files.FirstOrDefault();
                    if (profilePictureFile != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[AUTH REGISTER] Found file: {profilePictureFile.FileName}, Size: {profilePictureFile.Length} bytes");
                    }
                }

                if (profilePictureFile != null && profilePictureFile.Length > 0)
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine($"[AUTH REGISTER] Cloudinary service found, starting upload...");

                        // Create memory stream - don't use 'using' as it will dispose before async completes
                        var ms = new System.IO.MemoryStream();
                        await profilePictureFile.CopyToAsync(ms);
                        ms.Position = 0;

                        System.Diagnostics.Debug.WriteLine($"[AUTH REGISTER] File copied to memory stream, size: {ms.Length} bytes");

                        var uploadDto = new SV.Common.DTOs.FileUploadDto
                        {
                            Content = ms,
                            FileName = profilePictureFile.FileName,
                            ContentType = profilePictureFile.ContentType
                        };

                        var uploadResult = await _cloudinaryService.UploadImageAsync(uploadDto, "user-profiles");

                        System.Diagnostics.Debug.WriteLine($"[AUTH REGISTER] Upload completed. SecureUrl: {uploadResult?.SecureUrl}, PublicId: {uploadResult?.PublicId}");

                        // Dispose the stream after upload completes
                        ms.Dispose();

                        if (uploadResult != null && !string.IsNullOrEmpty(uploadResult.SecureUrl))
                        {
                            request.ProfileImageUrl = uploadResult.SecureUrl;
                            request.ProfileImagePublicId = uploadResult.PublicId;

                            System.Diagnostics.Debug.WriteLine($"[AUTH REGISTER] Profile image URL set in request DTO");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[AUTH REGISTER] Upload result was null or SecureUrl was empty");
                            return BadRequest("Cloudinary upload returned empty result");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[AUTH REGISTER] Exception during upload: {ex.GetType().Name} - {ex.Message}\n{ex.StackTrace}");
                        return BadRequest($"Profile picture upload failed: {ex.Message}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[AUTH REGISTER] No profile picture provided or file size is 0");
                }

                System.Diagnostics.Debug.WriteLine($"[AUTH REGISTER] Calling AuthService.RegisterAsync with ProfileImageUrl={request.ProfileImageUrl}, ProfileImagePublicId={request.ProfileImagePublicId}");

                var resp = await _authService.RegisterAsync(request);
                if (resp == null) return BadRequest();

                System.Diagnostics.Debug.WriteLine($"[AUTH REGISTER] Registration completed successfully");
                return Ok(resp);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AUTH REGISTER] Unhandled exception: {ex.GetType().Name} - {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, $"Register endpoint error: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPut("profile")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateProfile()
        {
            try
            {
                var userGuid = User.FindFirst("UserGuid")?.Value;
                if (string.IsNullOrEmpty(userGuid))
                {
                    return Unauthorized(new { error = "User identifier claim not found" });
                }

                IFormCollection form = null;
                try
                {
                    form = Request.Form;
                }
                catch (Exception ex)
                {
                    return BadRequest(new
                    {
                        error = $"Failed to access form: {ex.Message}",
                        type = ex.GetType().Name
                    });
                }

                if (form == null || (form.Count == 0 && form.Files.Count == 0))
                {
                    return BadRequest(new { error = "Form data is empty" });
                }

                // Parse optional form fields
                var fullName = form.ContainsKey("FullName") ? form["FullName"].ToString() : null;
                var mobile = form.ContainsKey("Mobile") ? form["Mobile"].ToString() : null;
                var country = form.ContainsKey("Country") ? form["Country"].ToString() : null;

                var request = new UpdateUserRequest
                {
                    FullName = fullName,
                    Mobile = mobile,
                    Country = country
                };

                IFormFile? profilePictureFile = null;
                if (form.Files.Count > 0)
                {
                    profilePictureFile = form.Files["ProfilePicture"] ?? form.Files.FirstOrDefault();
                }

                // Validate profile image format and size (max 10MB)
                if (profilePictureFile != null && profilePictureFile.Length > 0)
                {
                    var validation = ProjectFileStructure.Helpers.FileValidator.ValidateImage(profilePictureFile, "Profile Picture");
                    if (!validation.IsValid)
                    {
                        return BadRequest(new { error = validation.ErrorMessage });
                    }
                }

                SV.Common.DTOs.FileUploadDto? profilePictureDto = null;
                if (profilePictureFile != null && profilePictureFile.Length > 0)
                {
                    profilePictureDto = new SV.Common.DTOs.FileUploadDto
                    {
                        Content = profilePictureFile.OpenReadStream(),
                        FileName = profilePictureFile.FileName,
                        ContentType = profilePictureFile.ContentType
                    };
                }

                var success = await _userService.UpdateUserAsync(userGuid, request, profilePictureDto);
                if (!success)
                {
                    return NotFound(new { error = "User not found or failed to update profile" });
                }

                var updatedUser = await _userService.GetUserByGuidAsync(userGuid);
                return Ok(updatedUser);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Server error: {ex.Message}" });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resp = await _authService.ForgotPasswordAsync(request.Email);
            if (!resp.Success)
            {
                return BadRequest(new { success = false, message = resp.Message });
            }

            return Ok(new { success = true, message = resp.Message });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resp = await _authService.ResetPasswordAsync(request.Token, request.NewPassword);
            if (!resp.Success)
            {
                return BadRequest(new { success = false, message = resp.Message });
            }

            return Ok(new { success = true, message = resp.Message });
        }
    }
}

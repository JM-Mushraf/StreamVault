using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV.Service.Abstractions;

namespace ProjectFileStructure.Controllers;

[ApiController]
[Route("api/[controller]")]
/// <summary>
/// Dashboard endpoints: admin and user dashboards with aggregated metrics.
/// </summary>
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [Authorize(Roles = "1")]
    [HttpGet("admin")]
    public async Task<IActionResult> Admin()
    {
        var data = await _dashboardService.GetAdminDashboardAsync();
        return Ok(data);
    }

    [Authorize]
    [HttpGet("user")]
    public async Task<IActionResult> UserDashboard()
    {
        var userGuid = User.FindFirst("UserGuid")?.Value;
        if (string.IsNullOrWhiteSpace(userGuid)) return Unauthorized();

        var data = await _dashboardService.GetUserDashboardAsync(userGuid);
        return Ok(data);
    }
}

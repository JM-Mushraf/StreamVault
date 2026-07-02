using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV.Common.DTOs;

using SV.Service.Abstractions;

namespace ProjectFileStructure.Controllers;

[ApiController]
[Route("api/[controller]")]
/// <summary>
/// Plan endpoints: list available subscription plans and create new plans (admin only).
/// </summary>
public class PlanController : ControllerBase
{
    private readonly IPlanService _planService;

    public PlanController(IPlanService planService)
    {
        _planService = planService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var plans = await _planService.GetPlansAsync();
        return Ok(plans);
    }

    [Authorize(Roles = "1")] // ADMIN role id assumed as '1'
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePlanRequest request)
    {
        var createdBy = User.FindFirst("UserGuid")?.Value ?? User.Identity?.Name ?? "system";
        await _planService.CreatePlanAsync(request, createdBy);
        return Ok(new { success = true });
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV.Service.Abstractions;

namespace ProjectFileStructure.Controllers;

[ApiController]
[Route("api/[controller]")]
/// <summary>
/// Manage roles: create, update and soft-delete roles. Requires admin role for modifying operations.
/// </summary>
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [Authorize(Roles = "1")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SV.Common.DTOs.Role.CreateRoleDto request)
    {
        var createdBy = User.Identity?.Name ?? "SYSTEM";
        await _roleService.CreateRoleAsync(request.Name, createdBy);
        return Ok(new { success = true });
    }

    [Authorize(Roles = "1")]
    [HttpPut("{roleGuid}")]
    public async Task<IActionResult> Update(string roleGuid, [FromBody] SV.Common.DTOs.Role.UpdateRoleDto request)
    {
        var updatedBy = User.Identity?.Name ?? "SYSTEM";
        await _roleService.UpdateRoleAsync(roleGuid, request.Name, updatedBy);
        return Ok(new { success = true });
    }

    [Authorize(Roles = "1")]
    [HttpDelete("{roleGuid}")]
    public async Task<IActionResult> Delete(string roleGuid)
    {
        var updatedBy = User.Identity?.Name ?? "SYSTEM";
        await _roleService.DeleteRoleAsync(roleGuid, updatedBy);
        return Ok(new { success = true });
    }
}

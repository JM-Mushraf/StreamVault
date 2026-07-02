using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using Microsoft.Data.SqlClient;
using SV.Data.Connections;

namespace ProjectFileStructure.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public AdminController(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    [Authorize(Roles = "1")]
    [HttpPost("seed")] // Here seed is used to populate the database with initial data for roles, plans, and genres. This is typically done once during setup or testing.
    public IActionResult Seed()
    {
        try
        {
            using var conn = _dbConnectionFactory.CreateConnection();
            conn.Execute("usp_SeedRoles", commandType: System.Data.CommandType.StoredProcedure);
            conn.Execute("usp_SeedPlans", commandType: System.Data.CommandType.StoredProcedure);
            conn.Execute("usp_SeedGenres", commandType: System.Data.CommandType.StoredProcedure);
            return Ok(new { success = true });
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            // Duplicate key violation indicates seed has already been applied for those records
            return Ok(new { success = true, message = "Seed already applied (duplicate keys)." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An error occurred.", error = ex.Message });
        }
    }
}
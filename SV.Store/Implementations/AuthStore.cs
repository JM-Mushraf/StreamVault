using Dapper;
using SV.Common.DTOs.Auth;
using SV.Common.Utilities;
using SV.Data.Connections;
using SV.Store.Abstractions;

namespace SV.Store.Implementations;

public partial class AuthStore : IAuthStore
{
    protected readonly IDbConnectionFactory _dbConnectionFactory;

    public AuthStore(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        // Hash password and insert user using stored procedure
        var passwordHash = PasswordHelper.HashPassword(request.Password);
        using var connection = _dbConnectionFactory.CreateConnection();

        // Default RoleId for new users
        var defaultRoleId = 2;

        await connection.ExecuteAsync(
            SV.Common.Constants.AppConstants.SpInsertUser,
            new
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = passwordHash,
                Mobile = string.IsNullOrWhiteSpace(request.Mobile) ? null : request.Mobile,
                Country = string.IsNullOrWhiteSpace(request.Country) ? null : request.Country,
                RoleId = defaultRoleId,
                CreatedBy = string.IsNullOrWhiteSpace(request.FullName) ? "system" : request.FullName
            },
            commandType: System.Data.CommandType.StoredProcedure);

        // Retrieve the created user (usp_LoginUser returns the user row)
        var user = await connection.QueryFirstOrDefaultAsync<dynamic>(
            SV.Common.Constants.AppConstants.SpLoginUser,
            new { Email = request.Email },
            commandType: System.Data.CommandType.StoredProcedure);

        if (user == null)
        {
            return new AuthResponseDto
            {
                UserGuid = string.Empty,
                FullName = request.FullName,
                Email = request.Email,
                RoleId = defaultRoleId.ToString()
            };
        }

        return new AuthResponseDto
        {
            UserGuid = user.UserGuid ?? string.Empty,
            FullName = user.FullName ?? request.FullName ?? string.Empty,
            Email = user.Email ?? request.Email ?? string.Empty,
            RoleId = user.RoleId?.ToString() ?? defaultRoleId.ToString()
        };
    }

    public async Task<AuthResponseDto?> LoginAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return null;

        using var connection = _dbConnectionFactory.CreateConnection();
        try
        {
            var user = await connection.QueryFirstOrDefaultAsync<dynamic>(
                SV.Common.Constants.AppConstants.SpLoginUser,
                new { Email = email },
                commandType: System.Data.CommandType.StoredProcedure);

            if (user == null)
                return null;


            string storedHash = user.PasswordHash;

            if (!PasswordHelper.VerifyPassword(password, storedHash))
                return null;

            return new AuthResponseDto
            {
                UserGuid = user.UserGuid ?? string.Empty,
                FullName = user.FullName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                RoleId = user.RoleId?.ToString() ?? string.Empty
            };
        }
        catch (Microsoft.Data.SqlClient.SqlException)
        {
            return null;
        }
    }
}

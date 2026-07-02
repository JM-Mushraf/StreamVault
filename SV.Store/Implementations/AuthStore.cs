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

        // Ensure connection is open
        if (connection.State == System.Data.ConnectionState.Closed)
            connection.Open();

        // Default RoleId for new users
        var defaultRoleId = 2;

        var parameters = new
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = passwordHash,
            Mobile = string.IsNullOrWhiteSpace(request.Mobile) ? (object)DBNull.Value : request.Mobile,
            Country = string.IsNullOrWhiteSpace(request.Country) ? (object)DBNull.Value : request.Country,
            RoleId = defaultRoleId,
            CreatedBy = string.IsNullOrWhiteSpace(request.FullName) ? "system" : request.FullName,
            // Pass null values as DBNull.Value for the stored procedure
            UserProfileImageUrl = string.IsNullOrWhiteSpace(request.ProfileImageUrl) ? (object)DBNull.Value : request.ProfileImageUrl,
            UserProfileImagePublicId = string.IsNullOrWhiteSpace(request.ProfileImagePublicId) ? (object)DBNull.Value : request.ProfileImagePublicId
        };

        System.Diagnostics.Debug.WriteLine($"[STORE] RegisterAsync - ProfileImageUrl: {parameters.UserProfileImageUrl}, ProfileImagePublicId: {parameters.UserProfileImagePublicId}");

        try
        {
            // Execute insert and get the returned UserGuid
            var insertResult = await connection.QueryFirstOrDefaultAsync<dynamic>(
                SV.Common.Constants.AppConstants.SpInsertUser,
                parameters,
                commandType: System.Data.CommandType.StoredProcedure);

            if (insertResult == null)
            {
                System.Diagnostics.Debug.WriteLine($"[STORE] Insert result is NULL");
                throw new Exception("Failed to insert user - stored procedure returned no result");
            }

            System.Diagnostics.Debug.WriteLine($"[STORE] Insert result UserGuid: {insertResult.UserGuid}");

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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[STORE] RegisterAsync ERROR: {ex.GetType().Name} - {ex.Message}\n{ex.StackTrace}");
            throw;
        }
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

    public async Task<bool> SetPasswordResetTokenAsync(string email, string token, System.DateTime expiry)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        if (connection.State == System.Data.ConnectionState.Closed)
            connection.Open();

        var query = "UPDATE mst_User SET PasswordResetToken = @Token, PasswordResetExpiry = @Expiry, UpdatedOn = GETDATE(), UpdatedBy = 'SYSTEM' WHERE Email = @Email AND IsActive = 1";
        var rowsAffected = await connection.ExecuteAsync(query, new { Email = email, Token = token, Expiry = expiry });
        return rowsAffected > 0;
    }

    public async Task<bool> UpdatePasswordAndClearTokenAsync(string token, string passwordHash)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        if (connection.State == System.Data.ConnectionState.Closed)
            connection.Open();

        var query = "UPDATE mst_User SET PasswordHash = @PasswordHash, PasswordResetToken = NULL, PasswordResetExpiry = NULL, UpdatedOn = GETDATE(), UpdatedBy = 'SYSTEM' WHERE PasswordResetToken = @Token AND IsActive = 1";
        var rowsAffected = await connection.ExecuteAsync(query, new { Token = token, PasswordHash = passwordHash });
        return rowsAffected > 0;
    }

    public async Task<dynamic?> GetUserByResetTokenAsync(string token)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        if (connection.State == System.Data.ConnectionState.Closed)
            connection.Open();

        var query = "SELECT UserGuid, Email, PasswordResetExpiry FROM mst_User WHERE PasswordResetToken = @Token AND IsActive = 1";
        return await connection.QueryFirstOrDefaultAsync<dynamic>(query, new { Token = token });
    }
}

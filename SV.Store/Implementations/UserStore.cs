using System.Threading.Tasks;
using Dapper;
using System.Data;
using SV.Data.Connections;
using SV.Store.Abstractions;

namespace SV.Store.Implementations
{
    public class UserStore : IUserStore
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public UserStore(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<int?> GetUserIdByGuidAsync(string userGuid)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();
            var id = await conn.QueryFirstOrDefaultAsync<int?>("SELECT UserId FROM mst_User WHERE UserGuid = @UserGuid", new { UserGuid = userGuid }, commandType: CommandType.Text);
            return id;
        }

        public async Task<string?> GetUserGuidByIdAsync(int userId)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();
            var guid = await conn.QueryFirstOrDefaultAsync<string?>("SELECT UserGuid FROM mst_User WHERE UserId = @UserId", new { UserId = userId }, commandType: CommandType.Text);
            return guid;
        }

        public async Task<bool> UpdateUserAvatarAsync(string userGuid, string avatarUrl, string avatarPublicId)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            // Use stored procedure usp_UpdateUser which accepts UserProfileImageUrl and UserProfileImagePublicId as optional params
            var parameters = new DynamicParameters();
            parameters.Add("@UserGuid", userGuid);
            parameters.Add("@UserProfileImageUrl", avatarUrl);
            parameters.Add("@UserProfileImagePublicId", avatarPublicId);

            try
            {
                await conn.ExecuteAsync("dbo.usp_UpdateUser", parameters, commandType: CommandType.StoredProcedure);
                return true;
            }
            catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 50001)
            {
                return false;
            }
        }

        public async Task<SV.Common.DTOs.UserResponseDto?> GetUserByGuidAsync(string userGuid)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            var user = await conn.QueryFirstOrDefaultAsync<SV.Common.DTOs.UserResponseDto>("SELECT UserGuid, FullName, Email, RoleId, UserProfileImageUrl, UserProfileImagePublicId FROM mst_User WHERE UserGuid = @UserGuid AND IsActive = 1", new { UserGuid = userGuid }, commandType: CommandType.Text);
            if (user == null) return null;

            // Resolve RoleName from RoleId
            var roleName = await conn.QueryFirstOrDefaultAsync<string?>("SELECT RoleName FROM mst_Role WHERE RoleId = (SELECT RoleId FROM mst_User WHERE UserGuid = @UserGuid)", new { UserGuid = userGuid }, commandType: CommandType.Text);
            if (!string.IsNullOrEmpty(roleName)) user.RoleName = roleName;

            return user;
        }

        public async Task<bool> UpdateUserAsync(
            string userGuid,
            string? fullName,
            string? mobile,
            string? country,
            string updatedBy,
            string? userProfileImageUrl,
            string? userProfileImagePublicId)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            var parameters = new DynamicParameters();
            parameters.Add("@UserGuid", userGuid);
            parameters.Add("@FullName", fullName);
            parameters.Add("@Mobile", mobile);
            parameters.Add("@Country", country);
            parameters.Add("@UpdatedBy", updatedBy);
            parameters.Add("@UserProfileImageUrl", userProfileImageUrl);
            parameters.Add("@UserProfileImagePublicId", userProfileImagePublicId);

            try
            {
                await conn.ExecuteAsync("dbo.usp_UpdateUser", parameters, commandType: CommandType.StoredProcedure);
                return true;
            }
            catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 50001)
            {
                return false;
            }
        }
    }
}

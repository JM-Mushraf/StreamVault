using System.Threading.Tasks;
using Dapper;
using System.Data;
using SV.Store.Abstractions;
using SV.Data.Connections;
using SV.Common.Constants;

namespace SV.Store.Implementations
{
    public class RoleStore : IRoleStore
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public RoleStore(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<string> CreateAsync(string roleName, string createdBy)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            // usp_InsertRole should OUTPUT inserted.RoleGuid
            var guid = await conn.QuerySingleAsync<string>(AppConstants.SpInsertRole, new { RoleName = roleName, CreatedBy = createdBy }, commandType: CommandType.StoredProcedure);
            return guid;
        }

        public async Task UpdateAsync(string roleGuid, string roleName, string updatedBy)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            await conn.ExecuteAsync(AppConstants.SpUpdateRole, new { RoleGuid = roleGuid, RoleName = roleName, UpdatedBy = updatedBy }, commandType: CommandType.StoredProcedure);
        }

        public async Task DeleteAsync(string roleGuid, string updatedBy)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            await conn.ExecuteAsync(AppConstants.SpDeleteRole, new { RoleGuid = roleGuid, UpdatedBy = updatedBy }, commandType: CommandType.StoredProcedure);
        }

        public async Task<bool> ExistsByNameAsync(string roleName)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            var cnt = await conn.QuerySingleOrDefaultAsync<int>("SELECT COUNT(1) FROM mst_Role WHERE RoleName = @RoleName AND IsActive = 1", new { RoleName = roleName });
            return cnt > 0;
        }

        public async Task<System.Collections.Generic.List<object>> GetActiveAsync()
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            var rows = await conn.QueryAsync<dynamic>("SELECT RoleGuid, RoleName, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy FROM mst_Role WHERE IsActive = 1 ORDER BY RoleName");
            return rows.Select(r => (object)r).ToList();
        }
    }
}

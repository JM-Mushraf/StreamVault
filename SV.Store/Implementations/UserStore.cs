using System.Threading.Tasks;
using Dapper;
using SV.Store.Abstractions;
using SV.Data.Connections;
using System.Data;

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

            var id = await conn.QueryFirstOrDefaultAsync<int?>("SELECT UserId FROM mst_User WHERE UserGuid = @UserGuid AND IsActive = 1", new { UserGuid = userGuid }, commandType: CommandType.Text);
            return id;
        }

        public async Task<string?> GetUserGuidByIdAsync(int userId)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            var guid = await conn.QueryFirstOrDefaultAsync<string?>("SELECT UserGuid FROM mst_User WHERE UserId = @UserId AND IsActive = 1", new { UserId = userId }, commandType: CommandType.Text);
            return guid;
        }
    }
}

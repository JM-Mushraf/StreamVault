using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using System.Linq;
using SV.Store.Abstractions;
using SV.Data.Connections;

namespace SV.Store.Implementations
{
    public class GenreStore : IGenreStore
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public GenreStore(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<List<object>> GetGenresAsync()
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();
            var rows = await conn.QueryAsync<dynamic>("GetGenres", commandType: CommandType.StoredProcedure);
            return rows.Select(r => (object)r).ToList();
        }

        public async Task<int> CreateGenreAsync(string name, string createdBy)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();
            var id = await conn.QuerySingleAsync<int>(
                "CreateGenre",
                new { GenreName = name, CreatedBy = createdBy },
                commandType: CommandType.StoredProcedure);
            return id;
        }
    }
}

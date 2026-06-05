using System.Data;

namespace SV.Data.Connections;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
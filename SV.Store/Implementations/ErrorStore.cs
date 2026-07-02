using Dapper;
using SV.Data.Connections;
using SV.Store.Abstractions;

namespace SV.Store.Implementations
{
    public class ErrorStore : IErrorStore
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public ErrorStore(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task LogErrorAsync(string errorMessage, string errorProcedure, int errorLine, string stackTrace)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            await connection.ExecuteAsync(SV.Common.Constants.AppConstants.SpInsertErrorLog, new
            {
                ErrorMessage = errorMessage,
                ErrorProcedure = errorProcedure,
                ErrorLine = errorLine,
                StackTrace = stackTrace
            }, commandType: System.Data.CommandType.StoredProcedure);
        }
    }
}
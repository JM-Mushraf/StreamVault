using System.Threading.Tasks;

namespace SV.Store.Abstractions
{
    public interface IErrorStore
    {
        Task LogErrorAsync(string errorMessage, string errorProcedure, int errorLine, string stackTrace);
    }
}
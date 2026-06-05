using System.Threading.Tasks;

namespace SV.Store.Abstractions
{
    public interface IUserStore
    {
        Task<int?> GetUserIdByGuidAsync(string userGuid);
        Task<string?> GetUserGuidByIdAsync(int userId);
    }
}

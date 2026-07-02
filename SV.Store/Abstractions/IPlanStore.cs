using System.Collections.Generic;
using System.Threading.Tasks;

namespace SV.Store.Abstractions
{
    public interface IPlanStore
    {
        Task<List<object>> GetPlansAsync();
        Task CreatePlanAsync(SV.Common.DTOs.CreatePlanRequest request, string createdBy);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;

namespace SV.Service.Abstractions
{
    public interface IPlanService
    {
        Task<List<object>> GetPlansAsync();
        Task CreatePlanAsync(object request);
    }
}

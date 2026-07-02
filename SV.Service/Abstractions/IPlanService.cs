using System.Collections.Generic;
using System.Threading.Tasks;
using SV.Common.DTOs;

namespace SV.Service.Abstractions
{
    public interface IPlanService
    {
        Task<List<object>> GetPlansAsync();
        Task CreatePlanAsync(CreatePlanRequest request, string createdBy);
    }
}

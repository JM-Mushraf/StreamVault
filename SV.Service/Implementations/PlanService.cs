using System.Collections.Generic;
using System.Threading.Tasks;
using SV.Service.Abstractions;
using SV.Store.Abstractions;
using SV.Common.DTOs;

namespace SV.Service.Implementations
{
    public class PlanService : IPlanService
    {
        private readonly IPlanStore _planStore;

        public PlanService(IPlanStore planStore)
        {
            _planStore = planStore;
        }

        public Task<List<object>> GetPlansAsync()
        {
            return _planStore.GetPlansAsync();
        }

        public Task CreatePlanAsync(CreatePlanRequest request, string createdBy)
        {
            return _planStore.CreatePlanAsync(request, createdBy);
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using SV.Service.Abstractions;
using SV.Store.Abstractions;

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

        public Task CreatePlanAsync(object request)
        {
            return _planStore.CreatePlanAsync(request);
        }
    }
}

using System;

namespace SV.Common.DTOs
{
    public class SubscriptionCreateRequest
    {
        public int PlanId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}

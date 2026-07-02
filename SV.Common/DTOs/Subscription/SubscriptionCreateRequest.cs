using System;

namespace SV.Common.DTOs.Subscription
{
    public class SubscriptionCreateRequest
    {
        // Accept PlanGuid from client
        public string PlanGuid { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}

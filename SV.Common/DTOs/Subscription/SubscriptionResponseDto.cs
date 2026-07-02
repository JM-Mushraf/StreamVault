using System;

namespace SV.Common.DTOs.Subscription
{
    public class SubscriptionResponseDto
    {
        public string FullName { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}

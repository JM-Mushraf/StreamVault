using System;

namespace SV.Common.DTOs
{
    public class PaymentCreateRequest
    {
        public int SubscriptionId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string TransactionStatus { get; set; } = string.Empty;
        public DateTime PaidOn { get; set; }
    }
}

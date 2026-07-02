using System;

namespace SV.Common.DTOs
{
    public class CreatePlanRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int DurationMonths { get; set; }
    }
}



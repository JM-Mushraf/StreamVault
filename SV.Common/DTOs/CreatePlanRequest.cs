using System;

namespace SV.Common.DTOs
{
    public class CreatePlanRequest
    {
        // Minimal properties for creating a plan. Add or adjust fields as needed by service logic.
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int DurationMonths { get; set; }
    }
}



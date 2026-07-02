using System.Collections.Generic;

namespace SV.Common.DTOs
{
    public class ReviewItem
    {
        public int UserId { get; set; }
        public decimal Rating { get; set; }
        public string? ReviewText { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string CreatedOn { get; set; } = string.Empty;
    }

    public class ReviewWithMovieResponse
    {
        public object? Movie { get; set; }
        public List<ReviewItem> Reviews { get; set; } = new List<ReviewItem>();
    }
}

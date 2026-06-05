namespace SV.Common.DTOs
{
    public class ReviewCreateRequest
    {
        public int MovieId { get; set; }
        public decimal Rating { get; set; }
        public string? ReviewText { get; set; }
    }
}

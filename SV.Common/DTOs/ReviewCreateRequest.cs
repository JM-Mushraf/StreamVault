namespace SV.Common.DTOs
{
    public class ReviewCreateRequest
    {
        public string MovieGuid { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? ReviewText { get; set; }
    }
}

namespace SV.Common.DTOs.Movie
{
    public class MovieSuggestResponseDto
    {
        public string MovieGuid { get; set; } = string.Empty;
        public string MovieName { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public string GenreName { get; set; } = string.Empty;
    }
}

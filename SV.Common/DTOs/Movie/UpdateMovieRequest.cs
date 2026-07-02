namespace SV.Common.DTOs.Movie
{
    public class UpdateMovieRequest
    {
        public string? GenreGuid { get; set; }
        public string? MovieName { get; set; }
        public int? DurationMinutes { get; set; }
        public string? Language { get; set; }
        public int? Rating { get; set; }
    }
}

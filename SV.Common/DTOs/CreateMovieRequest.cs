using System;

namespace SV.Common.DTOs
{
    public class CreateMovieRequest
    {
        public int GenreId { get; set; }
        public string MovieName { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Language { get; set; } = string.Empty;
        public decimal? Rating { get; set; }
    }
}

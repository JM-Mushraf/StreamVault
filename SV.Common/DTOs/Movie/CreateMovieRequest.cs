using System;

namespace SV.Common.DTOs.Movie
{
    public class CreateMovieRequest
    {
        
        public string GenreGuid { get; set; } = string.Empty;
        public string MovieName { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Language { get; set; } = string.Empty;
        public int? Rating { get; set; }
    }
}

using System;

namespace SV.Common.DTOs.Movie
{
    public class MovieResponseDto
    {
        public string MovieGuid { get; set; } = string.Empty;
        public string MovieName { get; set; } = string.Empty;
        public DateTime ReleaseDate { get; set; }
        public int DurationMinutes { get; set; }
        public string Language { get; set; } = string.Empty;
        public int? Rating { get; set; }
        public string GenreName { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; } = string.Empty;

        // Cloudinary Media Fields
        public string? ThumbnailUrl { get; set; }
        public string? ThumbnailPublicId { get; set; }
        public string? MovieVideoUrl { get; set; }
        public string? MovieVideoPublicId { get; set; }
        public string? VideoTranscodeStatus { get; set; }
        public string? AvailableFormats { get; set; }
    }
}

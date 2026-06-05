using System;

namespace SV.Store.Models
{
    public class GenreDto
    {
        public int GenreId { get; set; }
        public string GenreName { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsActive { get; set; }
    }
}

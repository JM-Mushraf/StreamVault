using System;

namespace SV.Store.Models
{
    public class GenreDto
    {
        public string GenreName { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsActive { get; set; }
        public string GenreGuid { get; set; }
    }
}

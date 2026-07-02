using System.IO;

namespace SV.Common.DTOs
{
    public class FileUploadDto
    {
        public Stream Content { get; set; } = Stream.Null;
        public string FileName { get; set; } = string.Empty;
        public string? ContentType { get; set; }
    }
}

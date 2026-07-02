using System.Threading;
using System.Threading.Tasks;
using SV.Common.DTOs;

namespace SV.Common.Abstractions
{
    public class UploadResultDto
    {
        public string SecureUrl { get; set; } = string.Empty;
        public string PublicId { get; set; } = string.Empty;
        public string ResourceType { get; set; } = string.Empty;
    }

    public interface ICloudinaryService
    {
        Task<UploadResultDto> UploadImageAsync(FileUploadDto file, string folder, CancellationToken cancellationToken = default);
        Task<UploadResultDto> UploadVideoAsync(FileUploadDto file, string folder, CancellationToken cancellationToken = default);
        Task<bool> DeleteResourceAsync(string publicId, string resourceType = "image");
    }
}

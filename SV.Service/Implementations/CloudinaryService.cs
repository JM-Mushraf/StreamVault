using System.Threading;
using System.Threading.Tasks;
using SV.Common.Abstractions;
using SV.Common.DTOs;
using Microsoft.Extensions.Configuration;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace SV.Service.Implementations
{
    public class CloudinaryService : SV.Common.Abstractions.ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration configuration)
        {
            var section = configuration.GetSection("Cloudinary");
            var cloudName = section["CloudName"] ?? string.Empty;
            var apiKey = section["ApiKey"] ?? string.Empty;
            var apiSecret = section["ApiSecret"] ?? string.Empty;

            var acc = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(acc);
            _cloudinary.Api.Secure = true;
        }

        // IFormFile overloads removed to keep this project free of Microsoft.AspNetCore.Http dependency.

        public async Task<UploadResultDto> UploadVideoAsync(FileUploadDto file, string folder, CancellationToken cancellationToken = default)
        {
            using var ms = new System.IO.MemoryStream();
            await file.Content.CopyToAsync(ms, cancellationToken);
            ms.Position = 0;

            var uploadParams = new VideoUploadParams
            {
                File = new FileDescription(file.FileName, ms),
                Folder = folder,
                UseFilename = true,
                UniqueFilename = false,
                Overwrite = true,
                EagerTransforms = new System.Collections.Generic.List<Transformation>
                {
                    new Transformation().StreamingProfile("auto").FetchFormat("m3u8")
                },
                EagerAsync = false
            };

            var res = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

            string finalUrl = res.SecureUrl?.ToString() ?? string.Empty;
            if (res.JsonObj != null && res.JsonObj["eager"] != null)
            {
                var eager = res.JsonObj["eager"];
                if (eager.HasValues && eager.First != null)
                {
                    finalUrl = eager.First["secure_url"]?.ToString() ?? finalUrl;
                }
            }

            return new UploadResultDto
            {
                SecureUrl = finalUrl,
                PublicId = res.PublicId ?? string.Empty,
                ResourceType = res.ResourceType.ToString()
            };
        }

        // IFormFile overloads removed to keep this project free of Microsoft.AspNetCore.Http dependency.

        public async Task<UploadResultDto> UploadImageAsync(FileUploadDto file, string folder, CancellationToken cancellationToken = default)
        {
            using var ms = new System.IO.MemoryStream();
            await file.Content.CopyToAsync(ms, cancellationToken);
            ms.Position = 0;

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, ms),
                Folder = folder,
                UseFilename = true,
                UniqueFilename = false,
                Overwrite = true
            };

            var res = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

            return new UploadResultDto
            {
                SecureUrl = res.SecureUrl?.ToString() ?? string.Empty,
                PublicId = res.PublicId ?? string.Empty,
                ResourceType = res.ResourceType.ToString()
            };
        }

        public async Task<bool> DeleteResourceAsync(string publicId, string resourceType = "image")
        {
            var deletionParams = new DeletionParams(publicId)
            {
                ResourceType = resourceType == "video" ? ResourceType.Video : ResourceType.Image
            };

            var res = await _cloudinary.DestroyAsync(deletionParams);
            return res.Result == "ok" || res.Result == "not found";
        }
    }
}

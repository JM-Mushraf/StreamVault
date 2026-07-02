using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;

namespace ProjectFileStructure.Helpers
{
    public static class FileValidator
    {
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private static readonly string[] AllowedImageMimeTypes = { "image/jpeg", "image/jpg", "image/png", "image/webp" };

        private static readonly string[] AllowedVideoExtensions = { ".mp4", ".mkv", ".avi", ".mov" };
        private static readonly string[] AllowedVideoMimeTypes = { 
            "video/mp4", 
            "video/x-matroska", 
            "video/avi", 
            "video/msvideo", 
            "video/x-msvideo", 
            "video/quicktime" 
        };

        /// <summary>
        /// Validates an image file (e.g. thumbnail) format and size.
        /// </summary>
        public static (bool IsValid, string? ErrorMessage) ValidateImage(IFormFile? file, string fieldName, long maxBytes = 10 * 1024 * 1024)
        {
            if (file == null || file.Length == 0)
            {
                return (true, null); 
            }

            if (file.Length > maxBytes)
            {
                return (false, $"{fieldName} size exceeds the limit of {maxBytes / (1024 * 1024)}MB.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var contentType = file.ContentType.ToLowerInvariant();

            if (!AllowedImageExtensions.Contains(extension) || !AllowedImageMimeTypes.Contains(contentType))
            {
                return (false, $"{fieldName} must be a valid image format (JPG, JPEG, PNG, WEBP).");
            }

            return (true, null);
        }

        /// <summary>
        /// Validates a video file format and size.
        /// </summary>
        public static (bool IsValid, string? ErrorMessage) ValidateVideo(IFormFile? file, string fieldName, long maxBytes = 2L * 1024 * 1024 * 1024)
        {
            if (file == null || file.Length == 0)
            {
                return (true, null); 
            }

            if (file.Length > maxBytes)
            {
                return (false, $"{fieldName} size exceeds the limit of 2GB.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var contentType = file.ContentType.ToLowerInvariant();

            if (!AllowedVideoExtensions.Contains(extension) || !AllowedVideoMimeTypes.Contains(contentType))
            {
                return (false, $"{fieldName} must be a valid video format (MP4, MKV, AVI, MOV).");
            }

            return (true, null);
        }
    }
}

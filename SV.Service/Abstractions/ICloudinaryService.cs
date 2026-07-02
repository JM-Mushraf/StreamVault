using SV.Common.Abstractions;

namespace SV.Service.Abstractions
{
    // Re-export the cloudinary abstraction from SV.Common so existing references remain valid
    public interface ICloudinaryService : SV.Common.Abstractions.ICloudinaryService
    {
    }
}

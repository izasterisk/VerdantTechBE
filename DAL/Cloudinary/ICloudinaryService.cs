using Microsoft.AspNetCore.Http;

namespace Infrastructure.Cloudinary;

public interface ICloudinaryService
{
    Task<UploadResultDTO> UploadAsync(IFormFile file, string folder, CancellationToken ct = default);
    Task<List<UploadResultDTO>> UploadManyAsync(IEnumerable<IFormFile> files, string folder, CancellationToken ct = default);
    Task<bool> DeleteAsync(string publicId, CancellationToken ct = default);
}

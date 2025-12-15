using BLL.DTO.Cloudinary;
using Microsoft.AspNetCore.Http;

namespace BLL.Interfaces.Infrastructure;

public interface ICloudinaryService
{
    Task<UploadResultDTO> UploadAsync(IFormFile file, string folder, CancellationToken ct = default);
    Task<List<UploadResultDTO>> UploadManyAsync(IEnumerable<IFormFile> files, string folder, CancellationToken ct = default);
    Task<bool> DeleteAsync(string publicId, CancellationToken ct = default);
}

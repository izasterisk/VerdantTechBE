using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace Infrastructure.Cloudinary
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly CloudinaryDotNet.Cloudinary _cloudinary;
        private readonly CloudinaryOptions _opts;

        private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        { "jpg", "jpeg", "png", "gif", "bmp", "webp", "tiff", "ico", "svg", "pdf" };

        public CloudinaryService(IOptions<CloudinaryOptions> opts)
        {
            _opts = opts?.Value ?? throw new ArgumentNullException(nameof(opts));

            if (string.IsNullOrWhiteSpace(_opts.CloudName) ||
                string.IsNullOrWhiteSpace(_opts.ApiKey) ||
                string.IsNullOrWhiteSpace(_opts.ApiSecret))
            {
                throw new InvalidOperationException("Thiếu Cloudinary credentials. Kiểm tra .env / appsettings.");
            }

            var account = new Account(_opts.CloudName, _opts.ApiKey, _opts.ApiSecret);
            _cloudinary = new CloudinaryDotNet.Cloudinary(account) { Api = { Secure = true } };
        }

        public async Task<UploadResultDTO> UploadAsync(IFormFile file, string folder, CancellationToken ct = default)
        {
            if (file is null || file.Length == 0)
                throw new InvalidOperationException("File rỗng.");

            var ext = Path.GetExtension(file.FileName)?.Trim('.').ToLowerInvariant() ?? "";
            var isImage = (file.ContentType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) ?? false)
                          || ImageExtensions.Contains(ext);

            var targetFolder = BuildFolder(folder);

            await using var stream = file.OpenReadStream();

            if (isImage)
            {
                var p = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = targetFolder,
                    UseFilename = true,
                    UniqueFilename = true,
                    Overwrite = false,
                    Transformation = new Transformation()
                        .Width(2000).Height(2000).Crop("limit").Quality("auto")
                };

                // ❗ Thư viện không có overload nhận CancellationToken
                var r = await _cloudinary.UploadAsync(p);
                EnsureOk(r);

                return new UploadResultDTO
                {
                    PublicId = r.PublicId,
                    Url = r.SecureUrl?.ToString() ?? string.Empty,
                    PublicUrl = r.SecureUrl?.ToString() ?? string.Empty
                };
            }
            else
            {
                var p = new RawUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = targetFolder,
                    UseFilename = true,
                    UniqueFilename = true,
                    Overwrite = false
                };

                // ❗ Gọi UploadAsync với RawUploadParams (không có CancellationToken)
                var r = await _cloudinary.UploadAsync(p);
                EnsureOk(r);

                return new UploadResultDTO
                {
                    PublicId = r.PublicId,
                    Url = r.SecureUrl?.ToString() ?? string.Empty,
                    PublicUrl = r.SecureUrl?.ToString() ?? string.Empty
                };
            }
        }

        public async Task<List<UploadResultDTO>> UploadManyAsync(IEnumerable<IFormFile> files, string folder, CancellationToken ct = default)
        {
            var list = files?.ToList() ?? new List<IFormFile>();
            var results = new List<UploadResultDTO>(list.Count);

            foreach (var f in list)
            {
                ct.ThrowIfCancellationRequested();
                var one = await UploadAsync(f, folder, ct);
                results.Add(one);
            }

            return results;
        }

        public async Task<bool> DeleteAsync(string publicId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(publicId)) return false;

            // Thử xóa như ảnh
            var delImg = await _cloudinary.DestroyAsync(new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Image,
                Invalidate = true
            });

            if (string.Equals(delImg.Result, "ok", StringComparison.OrdinalIgnoreCase)) return true;

            // Nếu không phải ảnh, thử xóa như raw (pdf, docx, ...)
            var delRaw = await _cloudinary.DestroyAsync(new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Raw,
                Invalidate = true
            });

            return string.Equals(delRaw.Result, "ok", StringComparison.OrdinalIgnoreCase);
        }

        private static void EnsureOk(UploadResult r)
        {
            if (r is null || r.Error is not null)
                throw new InvalidOperationException(r?.Error?.Message ?? "Upload thất bại.");
        }

        private string BuildFolder(string subFolder)
        {
            var baseFolder = _opts.DefaultFolder?.Trim().Trim('/');
            var child = subFolder?.Trim().Trim('/');

            if (string.IsNullOrWhiteSpace(baseFolder) && string.IsNullOrWhiteSpace(child))
                return string.Empty;

            if (string.IsNullOrWhiteSpace(baseFolder)) return child!;
            if (string.IsNullOrWhiteSpace(child)) return baseFolder!;

            return $"{baseFolder}/{child}";
        }
    }
}

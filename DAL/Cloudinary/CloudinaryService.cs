using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Infrastructure.Cloudinary;

public sealed class CloudinaryService : ICloudinaryService
{
    private readonly ICloudinary _cloud;
    private readonly CloudinaryOptions _opts;

    private static readonly HashSet<string> ImageMime =
    [
        "image/jpeg","image/png","image/webp","image/gif"
    ];

    private static readonly HashSet<string> PdfMime = ["application/pdf"];

    public CloudinaryService(IOptions<CloudinaryOptions> options)
    {
        _opts = options.Value;
        _cloud = new Cloudinary(new Account(_opts.CloudName, _opts.ApiKey, _opts.ApiSecret))
        {
            Api = { Secure = true }
        };
    }

    public async Task<UploadResultDTO> UploadAsync(IFormFile file, string folder, CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            throw new InvalidOperationException("Empty file.");

        // quick validation (allow pdf OR image)
        var isPdf = PdfMime.Contains(file.ContentType);
        var isImage = ImageMime.Contains(file.ContentType);

        var path = $"{(_opts.DefaultFolder?.Trim('/') ?? "verdanttech")}/{folder.Trim('/')}";

        using var stream = file.OpenReadStream();

        RawUploadParams rawParams;
        ImageUploadParams imgParams;

        UploadResult rawResult;
        ImageUploadResult imgResult;

        if (isPdf)
        {
            rawParams = new RawUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = path,
                ResourceType = ResourceType.Raw, // pdf is raw
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };
            rawResult = await _cloud.UploadAsync(rawParams, ct);
            if (rawResult.StatusCode is not System.Net.HttpStatusCode.OK)
                throw new InvalidOperationException($"Cloudinary pdf upload failed: {rawResult.Error?.Message}");

            return new UploadResultDTO
            {
                PublicId = rawResult.PublicId,
                Url = rawResult.SecureUrl?.ToString() ?? string.Empty,
                PublicUrl = rawResult.SecureUrl?.ToString() ?? string.Empty
            };
        }

        if (isImage)
        {
            imgParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = path,
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };
            imgResult = await _cloud.UploadAsync(imgParams, ct);
            if (imgResult.StatusCode is not System.Net.HttpStatusCode.OK)
                throw new InvalidOperationException($"Cloudinary image upload failed: {imgResult.Error?.Message}");

            return new UploadResultDTO
            {
                PublicId = imgResult.PublicId,
                Url = imgResult.SecureUrl?.ToString() ?? string.Empty,
                PublicUrl = imgResult.SecureUrl?.ToString() ?? string.Empty
            };
        }

        throw new InvalidOperationException($"Unsupported file type: {file.ContentType}");
    }

    public async Task<List<UploadResultDTO>> UploadManyAsync(IEnumerable<IFormFile> files, string folder, CancellationToken ct = default)
    {
        var list = new List<UploadResultDTO>();
        foreach (var f in files)
        {
            list.Add(await UploadAsync(f, folder, ct));
        }
        return list;
    }

    public async Task<bool> DeleteAsync(string publicId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(publicId)) return false;

        // Try delete as image first, then raw
        var img = await _cloud.DestroyAsync(new DeletionParams(publicId), ct);
        if (img.Result == "ok") return true;

        var raw = await _cloud.DestroyAsync(new DeletionParams(publicId) { ResourceType = ResourceType.Raw }, ct);
        return raw.Result == "ok";
    }
}

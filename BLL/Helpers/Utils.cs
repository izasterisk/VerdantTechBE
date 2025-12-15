using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;
using BLL.DTO.Cloudinary;
using BLL.DTO.MediaLink;
using BLL.Interfaces.Infrastructure;
using Microsoft.AspNetCore.Http;

namespace BLL.Helpers;

public static class Utils
{
    private static readonly Regex RemoveMarks = new(@"\p{Mn}+", RegexOptions.Compiled);  // bỏ dấu kết hợp
    private static readonly Regex NonAlnumToDash = new(@"[^a-z0-9]+", RegexOptions.Compiled); // gom thành '-'

    /// <summary>
    /// Tạo slug từ chuỗi đầu vào (chuyển thành URL-friendly string)
    /// </summary>
    /// <param name="input">Chuỗi đầu vào cần chuyển thành slug</param>
    /// <returns>Chuỗi slug đã được xử lý</returns>
    public static string GenerateSlug(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        input = input.Replace('đ', 'd').Replace('Đ', 'D');
        var s = input.Normalize(NormalizationForm.FormD);
        s = RemoveMarks.Replace(s, "");          // bỏ dấu
        s = s.ToLowerInvariant();
        s = NonAlnumToDash.Replace(s, "-");      // mọi thứ không phải a-z0-9 -> '-'
        s = s.Trim('-');

        // Giới hạn độ dài tối đa 255 ký tự, cắt ở dấu '-' gần nhất
        if (s.Length > 255)
        {
            var cut = s.LastIndexOf('-', 255);
            if (cut > 0)
                s = s.Substring(0, cut);
            else
                s = s.Substring(0, 255);
            s = s.Trim('-');
        }
        return s;
    }

    // =====================================================================
    // FILE UPLOAD HELPERS FOR PRODUCTS
    // =====================================================================

    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        { "jpg", "jpeg", "png", "gif", "webp", "bmp" };

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB
    private const int MaxImagesCount = 5;

    /// <summary>
    /// Upload single manual file (PDF) lên Cloudinary
    /// </summary>
    /// <param name="cloudinaryService">Cloudinary service instance</param>
    /// <param name="manualFile">File PDF manual</param>
    /// <param name="folder">Thư mục lưu trên Cloudinary (VD: "products/manuals")</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Tuple (manualUrl, manualPublicUrl, manualPublicId) hoặc null nếu không có file</returns>
    /// <exception cref="InvalidOperationException">Nếu file không hợp lệ (không phải PDF hoặc quá lớn)</exception>
    public static async Task<(string? Url, string? PublicUrl, string? PublicId)?> UploadManualFileAsync(
        ICloudinaryService cloudinaryService,
        IFormFile? manualFile,
        string folder,
        CancellationToken ct = default)
    {
        if (manualFile == null)
            return null;

        // Validate file type: CHỈ chấp nhận PDF
        var isPdf = string.Equals(manualFile.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase) ||
                    manualFile.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);

        if (!isPdf)
        {
            throw new InvalidOperationException(
                "File manual phải là PDF. File được chọn không hợp lệ.");
        }

        // Validate file size
        if (manualFile.Length > MaxFileSizeBytes)
        {
            throw new InvalidOperationException(
                $"File manual không được vượt quá {MaxFileSizeBytes / (1024 * 1024)}MB. File hiện tại: {manualFile.Length / (1024 * 1024)}MB.");
        }

        if (manualFile.Length == 0)
        {
            throw new InvalidOperationException("File manual rỗng.");
        }

        var uploadResult = await cloudinaryService.UploadAsync(manualFile, folder, ct);
        if (uploadResult == null)
            return null;

        return (uploadResult.Url, uploadResult.PublicUrl, uploadResult.PublicId);
    }

    /// <summary>
    /// Upload nhiều hình ảnh lên Cloudinary và transform thành MediaLinkItemDTO list
    /// </summary>
    /// <param name="cloudinaryService">Cloudinary service instance</param>
    /// <param name="images">Danh sách hình ảnh cần upload</param>
    /// <param name="folder">Thư mục lưu trên Cloudinary (VD: "products/images")</param>
    /// <param name="purpose">Mục đích của ảnh (VD: "none", "productimage")</param>
    /// <param name="startIndex">Chỉ số bắt đầu cho SortOrder (mặc định = 0)</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>List MediaLinkItemDTO với thông tin ảnh đã upload</returns>
    /// <exception cref="InvalidOperationException">Nếu có file không phải ảnh hoặc quá số lượng</exception>
    public static async Task<List<MediaLinkItemDTO>> UploadImagesAsync(
        ICloudinaryService cloudinaryService,
        List<IFormFile>? images,
        string folder,
        string purpose = "none",
        int startIndex = 1,
        CancellationToken ct = default)
    {
        if (images == null || images.Count == 0)
            return new List<MediaLinkItemDTO>();

        // Validate số lượng ảnh
        if (images.Count > MaxImagesCount)
        {
            throw new InvalidOperationException(
                $"Chỉ được upload tối đa {MaxImagesCount} ảnh. Số lượng hiện tại: {images.Count}.");
        }

        // Validate từng file
        for (int i = 0; i < images.Count; i++)
        {
            var image = images[i];

            // Validate file size
            if (image.Length > MaxFileSizeBytes)
            {
                throw new InvalidOperationException(
                    $"Ảnh #{i + 1} vượt quá kích thước cho phép ({MaxFileSizeBytes / (1024 * 1024)}MB). File: {image.FileName}");
            }

            if (image.Length == 0)
            {
                throw new InvalidOperationException($"Ảnh #{i + 1} rỗng. File: {image.FileName}");
            }

            // Validate file type: CHỈ chấp nhận ảnh
            var extension = Path.GetExtension(image.FileName)?.TrimStart('.').ToLowerInvariant();
            var isImage = (image.ContentType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) ?? false) ||
                          (!string.IsNullOrEmpty(extension) && AllowedImageExtensions.Contains(extension));

            if (!isImage)
            {
                throw new InvalidOperationException(
                    $"File #{i + 1} không phải là ảnh hợp lệ. Chỉ chấp nhận: {string.Join(", ", AllowedImageExtensions)}. File: {image.FileName}");
            }
        }

        var uploadResults = await cloudinaryService.UploadManyAsync(images, folder, ct);

        return uploadResults.Select((x, index) => new MediaLinkItemDTO
        {
            ImagePublicId = x.PublicId,
            ImageUrl = x.Url,
            Purpose = purpose,
            SortOrder = startIndex + index
        }).ToList();
    }
}
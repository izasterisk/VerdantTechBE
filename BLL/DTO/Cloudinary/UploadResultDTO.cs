namespace BLL.DTO.Cloudinary;

public sealed class UploadResultDTO
{
    public string PublicId { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string PublicUrl { get; init; } = string.Empty;
}

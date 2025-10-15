namespace Infrastructure.Cloudinary;

public sealed class UploadResultDTO
{
    public string PublicId { get; init; } = string.Empty; // cloudinary public_id
    public string Url { get; init; } = string.Empty; // secured URL
    public string PublicUrl { get; init; } = string.Empty; // alias = Url (kept for your code compatibility)
}

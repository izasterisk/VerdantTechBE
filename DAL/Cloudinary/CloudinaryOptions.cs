namespace Infrastructure.Cloudinary;

public sealed class CloudinaryOptions
{
    public string CloudName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;

    // Optional default folder for everything
    public string DefaultFolder { get; set; } = "verdanttech";
}

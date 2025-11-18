namespace DAL.Data.Models;

public partial class Crop
{
    public ulong Id { get; set; }

    public ulong FarmProfileId { get; set; }

    public string CropName { get; set; } = null!;

    public DateOnly PlantingDate { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual FarmProfile FarmProfile { get; set; } = null!;
}

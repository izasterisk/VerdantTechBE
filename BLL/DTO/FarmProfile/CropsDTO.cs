using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.FarmProfile;

public class CropsDTO
{
    public ulong Id { get; set; }

    // public long FarmProfileId { get; set; }

    public string CropName { get; set; } = null!;

    public DateOnly PlantingDate { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public class CropsCreateDTO
{
    // public long Id { get; set; }

    // public long FarmProfileId { get; set; }

    [StringLength(255, ErrorMessage = "Tên cây trồng không được vượt quá 255 ký tự")]
    public string CropName { get; set; } = null!;

    public DateOnly PlantingDate { get; set; }

    // public bool IsActive { get; set; }

    // public DateTime CreatedAt { get; set; }

    // public DateTime UpdatedAt { get; set; }
}

public class CropsUpdateDTO
{
    [Required(ErrorMessage = "ID không được để trống")]
    public ulong Id { get; set; }

    // public long FarmProfileId { get; set; }

    [StringLength(255, ErrorMessage = "Tên cây trồng không được vượt quá 255 ký tự")]
    public string? CropName { get; set; } = null!;

    public DateOnly? PlantingDate { get; set; }

    public bool? IsActive { get; set; } = true;

    // public DateTime CreatedAt { get; set; }

    // public DateTime UpdatedAt { get; set; }
}
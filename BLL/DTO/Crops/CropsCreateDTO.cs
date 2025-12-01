using DAL.Data;
using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.Crops;

public class CropsCreateDTO
{
    // public ulong Id { get; set; }

    // public ulong FarmProfileId { get; set; }

    [Required(ErrorMessage = "Tên cây trồng là bắt buộc")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Tên cây trồng phải có từ 1 đến 255 ký tự")]
    public string CropName { get; set; } = null!;

    [Required(ErrorMessage = "Ngày trồng là bắt buộc")]
    public DateOnly PlantingDate { get; set; }

    [Required(ErrorMessage = "Phương pháp trồng là bắt buộc")]
    [EnumDataType(typeof(PlantingMethod), ErrorMessage = "Phương pháp trồng không hợp lệ")]
    public PlantingMethod PlantingMethod { get; set; }

    [Required(ErrorMessage = "Loại cây trồng là bắt buộc")]
    [EnumDataType(typeof(CropType), ErrorMessage = "Loại cây trồng không hợp lệ")]
    public CropType CropType { get; set; }

    [Required(ErrorMessage = "Loại hình canh tác là bắt buộc")]
    [EnumDataType(typeof(FarmingType), ErrorMessage = "Loại hình canh tác không hợp lệ")]
    public FarmingType FarmingType { get; set; }

    [Required(ErrorMessage = "Trạng thái là bắt buộc")]
    [EnumDataType(typeof(CropStatus), ErrorMessage = "Trạng thái không hợp lệ")]
    public CropStatus Status { get; set; }

    // public DateTime CreatedAt { get; set; }

    // public DateTime UpdatedAt { get; set; }
}
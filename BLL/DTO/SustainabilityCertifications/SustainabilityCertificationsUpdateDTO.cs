using System.ComponentModel.DataAnnotations;
using DAL.Data;

namespace BLL.DTO.SustainabilityCertifications;

public class SustainabilityCertificationsUpdateDTO
{
    // public ulong? Id { get; set; }

    // [Required(ErrorMessage = "Mã chứng chỉ là bắt buộc")]
    [StringLength(100, ErrorMessage = "Mã chứng chỉ không được vượt quá {1} ký tự")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "Mã chứng chỉ chỉ được chứa chữ hoa, số và dấu gạch dưới")]
    public string? Code { get; set; } = string.Empty;

    // [Required(ErrorMessage = "Tên chứng chỉ là bắt buộc")]
    [StringLength(255, MinimumLength = 2, ErrorMessage = "Tên chứng chỉ phải từ {2} đến {1} ký tự")]
    public string? Name { get; set; } = string.Empty;

    // [Required(ErrorMessage = "Danh mục chứng chỉ là bắt buộc")]
    [EnumDataType(typeof(SustainabilityCertificationCategory), ErrorMessage = "Danh mục chứng chỉ không hợp lệ")]
    public SustainabilityCertificationCategory? Category { get; set; }

    [StringLength(255, ErrorMessage = "Tên tổ chức cấp chứng chỉ không được vượt quá 255 ký tự")]
    public string? IssuingBody { get; set; }

    [StringLength(5000, ErrorMessage = "Mô tả không được vượt quá 5000 ký tự")]
    public string? Description { get; set; }

    public bool? IsActive { get; set; } = true;

    // public DateTime? CreatedAt { get; set; }

    // public DateTime? UpdatedAt { get; set; }
}
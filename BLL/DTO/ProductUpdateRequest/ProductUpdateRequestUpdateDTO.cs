using System.ComponentModel.DataAnnotations;
using DAL.Data;

namespace BLL.DTO.ProductUpdateRequest;

public class ProductUpdateRequestUpdateDTO
{
    [Required(ErrorMessage = "Trạng thái là bắt buộc")]
    [EnumDataType(typeof(ProductRegistrationStatus), ErrorMessage = "Giá trị trạng thái không hợp lệ")]
    public ProductRegistrationStatus Status { get; set; }

    [MaxLength(500, ErrorMessage = "Lý do từ chối không được vượt quá 500 ký tự")]
    public string? RejectionReason { get; set; }
}
using System.ComponentModel.DataAnnotations;
using DAL.Data;

namespace BLL.DTO.Request;

public class RequestCreateDTO
{
    // public ulong Id { get; set; }

    // [Required(ErrorMessage = "User ID là bắt buộc")]
    // public ulong UserId { get; set; }

    [Required(ErrorMessage = "Loại yêu cầu là bắt buộc")]
    [EnumDataType(typeof(RequestType), ErrorMessage = "RequestType phải là một trong các giá trị hợp lệ: " +
                                                      "RefundRequest, SupportRequest.")]
    public RequestType RequestType { get; set; }

    [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
    [StringLength(255, MinimumLength = 3, ErrorMessage = "Tiêu đề phải từ 3 đến 255 ký tự")]
    public string Title { get; set; } = null!;

    [Required(ErrorMessage = "Mô tả là bắt buộc")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Mô tả phải từ 10 đến 2000 ký tự")]
    public string Description { get; set; } = null!;

    // public RequestStatus Status { get; set; } = RequestStatus.Pending;

    // public ulong? ProcessedBy { get; set; }

    // public DateTime? ProcessedAt { get; set; }

    // public DateTime CreatedAt { get; set; }

    // public DateTime UpdatedAt { get; set; }
    
    public List<RequestImageDTO>? Images { get; set; }
}

public class RequestImageDTO
{
    [Required(ErrorMessage = "imageURL là bắt buộc")]
    public string ImageUrl { get; set; } = null!;
    
    [Required(ErrorMessage = "publicURL là bắt buộc")]
    public string ImagePublicId { get; set; } = null!;
}
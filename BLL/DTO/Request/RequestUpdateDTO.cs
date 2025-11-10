using System.ComponentModel.DataAnnotations;
using DAL.Data;

namespace BLL.DTO.Request;

public class RequestUpdateDTO
{
    // public ulong Id { get; set; }

    // [Required(ErrorMessage = "User ID là bắt buộc")]
    // public ulong UserId { get; set; }

    // [Required(ErrorMessage = "Loại yêu cầu là bắt buộc")]
    // public RequestType RequestType { get; set; }

    // [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
    // [StringLength(255, MinimumLength = 3, ErrorMessage = "Tiêu đề phải từ 3 đến 255 ký tự")]
    // public string Title { get; set; } = null!;

    // [Required(ErrorMessage = "Mô tả là bắt buộc")]
    // [StringLength(2000, MinimumLength = 10, ErrorMessage = "Mô tả phải từ 10 đến 2000 ký tự")]
    // public string Description { get; set; } = null!;

    [EnumDataType(typeof(RequestStatus), ErrorMessage = "Trạng thái phải là 1 trong InReview, Approved, Rejected, Cancelled.")]
    public RequestStatus Status { get; set; } = RequestStatus.Pending;

    [StringLength(1000, ErrorMessage = "Ghi chú trả lời không được vượt quá 1000 ký tự")]
    public string? ReplyNotes { get; set; }

    // public ulong? ProcessedBy { get; set; }

    // public DateTime? ProcessedAt { get; set; }

    // public DateTime CreatedAt { get; set; }

    // public DateTime UpdatedAt { get; set; }
}
using System.ComponentModel.DataAnnotations;
using DAL.Data;

namespace BLL.DTO.Request;

public class RequestProcessDTO
{
    // public ulong Id { get; set; }

    // [Required(ErrorMessage = "User ID là bắt buộc")]
    // public ulong UserId { get; set; }

    // [Required(ErrorMessage = "Loại yêu cầu là bắt buộc")]
    // public RequestType RequestType { get; set; }

    // [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
    // [StringLength(255, MinimumLength = 3, ErrorMessage = "Tiêu đề phải từ 3 đến 255 ký tự")]
    // public string Title { get; set; } = null!;

    [EnumDataType(typeof(RequestStatus), ErrorMessage = "Trạng thái phải là 1 trong InReview, Approved, Rejected, Cancelled.")]
    public RequestStatus? Status { get; set; } = RequestStatus.Pending;

    // public ulong? ProcessedBy { get; set; }

    // public DateTime? ProcessedAt { get; set; }

    // public DateTime CreatedAt { get; set; }

    // public DateTime UpdatedAt { get; set; }
    
    public RequestMessageProcessDTO? RequestMessages { get; set; }
}

public class RequestMessageProcessDTO
{
    [Required(ErrorMessage = "Id tin nhắn là bắt buộc")]
    [Range(0, ulong.MaxValue, ErrorMessage = "Id không được âm.")]
    public ulong Id { get; set; }
    
    [Required(ErrorMessage = "Nội dung tin nhắn là bắt buộc")]
    [StringLength(1000, ErrorMessage = "Ghi chú trả lời không được vượt quá 1000 ký tự")]
    public string ReplyNotes { get; set; } = null!;
}
using System.ComponentModel.DataAnnotations;
using BLL.DTO.User;
using DAL.Data;

namespace BLL.DTO.Request;

public class RequestResponseDTO
{
    public ulong Id { get; set; }

    // [Required(ErrorMessage = "User ID là bắt buộc")]
    // public ulong UserId { get; set; }
    public UserResponseDTO User { get; set; } = null!;

    // [Required(ErrorMessage = "Loại yêu cầu là bắt buộc")]
    public RequestType RequestType { get; set; }

    // [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
    // [StringLength(255, MinimumLength = 3, ErrorMessage = "Tiêu đề phải từ 3 đến 255 ký tự")]
    public string Title { get; set; } = null!;

    public RequestStatus Status { get; set; } = RequestStatus.Pending;

    // public ulong? ProcessedBy { get; set; }
    public UserResponseDTO? ProcessedBy { get; set; } = null!;

    public DateTime? ProcessedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
    
    public List<RequestMessageResponseDTO>? RequestMessages { get; set; }
}
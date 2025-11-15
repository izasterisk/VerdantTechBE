using System.ComponentModel.DataAnnotations;
using DAL.Data;

namespace BLL.DTO.ChatbotConversations;

public class ChatbotMessageCreateDTO
{
    // public ulong Id { get; set; }

    // public ulong ConversationId { get; set; }

    [Required(ErrorMessage = "Loại tin nhắn là bắt buộc")]
    [EnumDataType(typeof(MessageType), ErrorMessage = "Loại tin nhắn không hợp lệ")]
    public MessageType MessageType { get; set; }

    [Required(ErrorMessage = "Nội dung tin nhắn là bắt buộc")]
    [StringLength(10000, MinimumLength = 1, ErrorMessage = "Nội dung tin nhắn phải từ 1 đến 10000 ký tự")]
    public string MessageText { get; set; } = null!;

    // public DateTime CreatedAt { get; set; }
}
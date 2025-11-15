using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.ChatbotConversations;

public class ChatbotConversationUpdateDTO
{
    // [Required(ErrorMessage = "ID cuộc hội thoại là bắt buộc")]
    // public ulong Id { get; set; }

    // public ulong CustomerId { get; set; }

    [StringLength(255, ErrorMessage = "Tiêu đề không được vượt quá 255 ký tự")]
    public string? Title { get; set; }
    
    [StringLength(5000, ErrorMessage = "Ngữ cảnh không được vượt quá 5000 ký tự")]
    public string? Context { get; set; }

    public bool IsActive { get; set; } = true;

    // public DateTime StartedAt { get; set; }
}
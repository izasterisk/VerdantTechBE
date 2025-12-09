using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.SurveyResponse;

public class SurveyResponseDTO
{
    // public ulong Id { get; set; }
    
    public ulong QuestionId { get; set; }
    
    public string? TextAnswer { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public class SurveyResponseCreateDTO
{
    [Required(ErrorMessage = "ID trang trại là bắt buộc")]
    public ulong FarmProfileId { get; set; }
    
    [Required(ErrorMessage = "Danh sách câu hỏi là bắt buộc")]
    [MinLength(10, ErrorMessage = "Phải trả đủ 10 câu hỏi")]
    [MaxLength(10, ErrorMessage = "Phải trả đủ 10 câu hỏi")]
    public List<AnswersDTO> Answers { get; set; } = null!;
}

public class AnswersDTO
{
    [Required(ErrorMessage = "ID câu hỏi là bắt buộc")]
    public ulong QuestionId { get; set; }
    
    [Required(ErrorMessage = "Câu trả lời là bắt buộc")]
    [StringLength(5000, ErrorMessage = "Câu trả lời không được vượt quá 5000 ký tự")]
    public string TextAnswer { get; set; } = null!;
}
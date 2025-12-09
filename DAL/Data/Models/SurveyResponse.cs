namespace DAL.Data.Models;

/// <summary>
/// Bảng lưu câu trả lời đánh giá độ bền vững của người dùng
/// </summary>
public class SurveyResponse
{
    public ulong Id { get; set; }

    /// <summary>
    /// Trang trại được đánh giá
    /// </summary>
    public ulong? FarmProfileId { get; set; }

    /// <summary>
    /// Câu hỏi được trả lời
    /// </summary>
    public ulong QuestionId { get; set; }

    /// <summary>
    /// Câu trả lời dạng text - bắt buộc nếu question_type = text
    /// </summary>
    public string? TextAnswer { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public virtual FarmProfile? FarmProfile { get; set; }
}

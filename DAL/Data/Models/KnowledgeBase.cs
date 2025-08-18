using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Knowledge base for AI chatbot responses
/// </summary>
public partial class KnowledgeBase
{
    public ulong Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Category { get; set; } = null!;

    [StringLength(100)]
    public string? Subcategory { get; set; }

    [Required]
    public string Question { get; set; } = null!;

    [Required]
    public string Answer { get; set; } = null!;

    /// <summary>
    /// Array of keywords for matching (JSON)
    /// </summary>
    public List<string> Keywords { get; set; } = new();

    public Language Language { get; set; } = Language.Vi;

    [StringLength(500)]
    public string? SourceUrl { get; set; }

    public bool IsVerified { get; set; } = false;

    public ulong? VerifiedBy { get; set; }

    public long UsageCount { get; set; } = 0L;

    public int HelpfulCount { get; set; } = 0;

    public int UnhelpfulCount { get; set; } = 0;

    public ulong? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual User? VerifiedByNavigation { get; set; }
    public virtual User? CreatedByNavigation { get; set; }
}

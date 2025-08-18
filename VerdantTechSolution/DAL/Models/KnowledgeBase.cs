using System;
using System.Collections.Generic;

namespace DAL.Models;

/// <summary>
/// Knowledge base for AI chatbot responses
/// </summary>
public partial class KnowledgeBase
{
    public ulong Id { get; set; }

    public string Category { get; set; } = null!;

    public string? Subcategory { get; set; }

    public string Question { get; set; } = null!;

    public string Answer { get; set; } = null!;

    /// <summary>
    /// Array of keywords for matching
    /// </summary>
    public string? Keywords { get; set; }

    public string? Language { get; set; }

    public string? SourceUrl { get; set; }

    public bool? IsVerified { get; set; }

    public ulong? VerifiedBy { get; set; }

    public long? UsageCount { get; set; }

    public int? HelpfulCount { get; set; }

    public int? UnhelpfulCount { get; set; }

    public ulong? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual User? VerifiedByNavigation { get; set; }
}

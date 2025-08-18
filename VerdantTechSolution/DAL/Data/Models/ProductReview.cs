using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Product reviews and ratings
/// </summary>
public partial class ProductReview
{
    public ulong Id { get; set; }

    public ulong ProductId { get; set; }

    public ulong OrderId { get; set; }

    public ulong CustomerId { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [StringLength(255)]
    public string? Title { get; set; }

    public string? Comment { get; set; }

    /// <summary>
    /// Array of review image URLs (JSON)
    /// </summary>
    public List<string> Images { get; set; } = new();

    public bool IsVerifiedPurchase { get; set; } = true;

    public int HelpfulCount { get; set; } = 0;

    public int UnhelpfulCount { get; set; } = 0;

    public string? VendorReply { get; set; }

    public DateTime? VendorRepliedAt { get; set; }

    public bool IsFeatured { get; set; } = false;

    public ReviewStatus Status { get; set; } = ReviewStatus.Pending;

    public ulong? ModeratedBy { get; set; }

    public DateTime? ModeratedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual Product Product { get; set; } = null!;
    public virtual Order Order { get; set; } = null!;
    public virtual User Customer { get; set; } = null!;
    public virtual User? ModeratedByNavigation { get; set; }
}

using System;
using System.Collections.Generic;

namespace DAL.Models;

/// <summary>
/// Product reviews and ratings
/// </summary>
public partial class ProductReview
{
    public ulong Id { get; set; }

    public ulong ProductId { get; set; }

    public ulong OrderId { get; set; }

    public ulong CustomerId { get; set; }

    public int Rating { get; set; }

    public string? Title { get; set; }

    public string? Comment { get; set; }

    /// <summary>
    /// Array of review image URLs
    /// </summary>
    public string? Images { get; set; }

    public bool? IsVerifiedPurchase { get; set; }

    public int? HelpfulCount { get; set; }

    public int? UnhelpfulCount { get; set; }

    public string? VendorReply { get; set; }

    public DateTime? VendorRepliedAt { get; set; }

    public bool? IsFeatured { get; set; }

    public string? Status { get; set; }

    public ulong? ModeratedBy { get; set; }

    public DateTime? ModeratedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User Customer { get; set; } = null!;

    public virtual User? ModeratedByNavigation { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}

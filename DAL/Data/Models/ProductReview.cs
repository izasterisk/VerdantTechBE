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

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual Product Product { get; set; } = null!;
    public virtual Order Order { get; set; } = null!;
    public virtual User Customer { get; set; } = null!;
}

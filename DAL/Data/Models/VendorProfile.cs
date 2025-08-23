using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Vendor/seller profile and verification details
/// </summary>
public partial class VendorProfile
{
    public ulong Id { get; set; }

    public ulong UserId { get; set; }

    [Required]
    [StringLength(255)]
    public string CompanyName { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Slug { get; set; } = null!;

    [StringLength(100)]
    public string? BusinessRegistrationNumber { get; set; }

    [StringLength(50)]
    public string? TaxCode { get; set; }

    public string? CompanyAddress { get; set; }

    /// <summary>
    /// JSON array of sustainability certifications
    /// </summary>
    public List<string> SustainabilityCredentials { get; set; } = new();

    public DateTime? VerifiedAt { get; set; }

    public ulong? VerifiedBy { get; set; }

    /// <summary>
    /// Encrypted bank details for payments (JSON)
    /// </summary>
    public Dictionary<string, object> BankAccountInfo { get; set; } = new();

    /// <summary>
    /// Platform commission percentage
    /// </summary>
    public decimal CommissionRate { get; set; } = 10.00m;

    public decimal RatingAverage { get; set; } = 0.00m;

    public int TotalReviews { get; set; } = 0;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual User? VerifiedByNavigation { get; set; }
}

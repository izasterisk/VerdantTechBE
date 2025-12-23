using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    [StringLength(255)]
    public string? Notes { get; set; }

    public bool SubscriptionActive { get; set; } = false;

    public DateTime? VerifiedAt { get; set; }

    public ulong? VerifiedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual User? VerifiedByNavigation { get; set; }
    public virtual ICollection<VendorCertificate> VendorCertificates { get; set; } = new List<VendorCertificate>();
    public virtual ICollection<BatchInventory> BatchInventories { get; set; } = new List<BatchInventory>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    [NotMapped]
    public List<MediaLink> MediaLinks { get; set; } = new();
}

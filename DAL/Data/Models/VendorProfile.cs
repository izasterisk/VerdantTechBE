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

    public string? CompanyAddress { get; set; }

    [StringLength(100)]
    public string? Province { get; set; }

    [StringLength(100)]
    public string? District { get; set; }

    [StringLength(100)]
    public string? Commune { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public ulong? VerifiedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual User? VerifiedByNavigation { get; set; }
    public virtual ICollection<VendorCertificate> VendorCertificates { get; set; } = new List<VendorCertificate>();
    public virtual ICollection<VendorBankAccount> VendorBankAccounts { get; set; } = new List<VendorBankAccount>();
    public virtual Wallet? Wallet { get; set; }
    public virtual ICollection<BatchInventory> BatchInventories { get; set; } = new List<BatchInventory>();
    public virtual ICollection<Cashout> Cashouts { get; set; } = new List<Cashout>();
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}

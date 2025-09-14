using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Bank accounts of vendor profiles
/// </summary>
public partial class VendorBankAccount
{
    public ulong Id { get; set; }

    public ulong VendorId { get; set; }

    [Required]
    [StringLength(20)]
    public string BankCode { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string AccountNumber { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string AccountHolder { get; set; } = null!;

    public bool IsDefault { get; set; } = false;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation
    public virtual VendorProfile VendorProfile { get; set; } = null!;
}



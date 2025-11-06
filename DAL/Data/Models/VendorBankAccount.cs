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

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation
    public virtual User Vendor { get; set; } = null!;
    public virtual ICollection<Cashout> Cashouts { get; set; } = new List<Cashout>();
}



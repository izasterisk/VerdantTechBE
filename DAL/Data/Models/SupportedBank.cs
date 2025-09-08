using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// List of supported banks
/// </summary>
public partial class SupportedBank
{
    public ulong Id { get; set; }

    [Required]
    [StringLength(20)]
    public string BankCode { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string BankName { get; set; } = null!;

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual ICollection<VendorBankAccount> VendorBankAccounts { get; set; } = new List<VendorBankAccount>();
}



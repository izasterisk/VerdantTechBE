using System.ComponentModel.DataAnnotations;
using DAL.Data;

namespace DAL.Data.Models;

/// <summary>
/// Vendor payout requests to bank accounts
/// </summary>
public partial class Payout
{
    public ulong Id { get; set; }

    public ulong VendorId { get; set; }

    public decimal Amount { get; set; }

    [Required]
    [StringLength(20)]
    public string BankCode { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string BankAccountNumber { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string BankAccountHolder { get; set; } = null!;

    public PayoutStatus Status { get; set; } = PayoutStatus.Pending;

    [StringLength(255)]
    public string? TransactionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime RequestedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation
    public virtual VendorProfile VendorProfile { get; set; } = null!;
    public virtual SupportedBank Bank { get; set; } = null!;
}



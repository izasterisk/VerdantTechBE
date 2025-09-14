using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Money going out - vendor payouts and expenses with banking details
/// </summary>
public partial class Cashout
{
    public ulong Id { get; set; }

    public ulong VendorId { get; set; }

    public ulong? TransactionId { get; set; }

    public decimal Amount { get; set; }

    [StringLength(20)]
    public string BankCode { get; set; } = null!;

    [StringLength(50)]
    public string BankAccountNumber { get; set; } = null!;

    [StringLength(255)]
    public string BankAccountHolder { get; set; } = null!;

    public CashoutStatus Status { get; set; } = CashoutStatus.Pending;

    public CashoutType CashoutType { get; set; } = CashoutType.CommissionPayout;

    [StringLength(255)]
    public string? GatewayTransactionId { get; set; }

    [StringLength(50)]
    public string? ReferenceType { get; set; }

    public ulong? ReferenceId { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public ulong? ProcessedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual VendorProfile Vendor { get; set; } = null!;
    public virtual Transaction? Transaction { get; set; }
    public virtual User? ProcessedByNavigation { get; set; }
}

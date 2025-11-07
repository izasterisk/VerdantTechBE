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

    public ulong BankAccountId { get; set; }

    public decimal Amount { get; set; }

    public CashoutStatus Status { get; set; } = CashoutStatus.Pending;

    [StringLength(255)]
    public string? Reason { get; set; }

    [StringLength(255)]
    public string? GatewayTransactionId { get; set; }

    public CashoutReferenceType? ReferenceType { get; set; }

    public ulong? ReferenceId { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public ulong? ProcessedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual User Vendor { get; set; } = null!;
    public virtual Transaction? Transaction { get; set; }
    public virtual VendorBankAccount BankAccount { get; set; } = null!;
    public virtual User? ProcessedByNavigation { get; set; }
}

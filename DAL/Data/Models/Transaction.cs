using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Central financial ledger - single source of truth for all money movements
/// </summary>
public partial class Transaction
{
    public ulong Id { get; set; }

    public TransactionType TransactionType { get; set; }

    public decimal Amount { get; set; }

    [StringLength(3)]
    public string Currency { get; set; } = "VND";

    // Core references
    public ulong? OrderId { get; set; }
    public ulong? CustomerId { get; set; }
    public ulong? VendorId { get; set; }

    // Wallet related fields
    public ulong? WalletId { get; set; }
    public decimal? BalanceBefore { get; set; }
    public decimal? BalanceAfter { get; set; }

    // Status and metadata
    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

    [StringLength(255)]
    public string Description { get; set; } = null!;

    /// <summary>
    /// Additional transaction metadata (JSON)
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    // Reference to domain-specific tables
    [StringLength(50)]
    public string? ReferenceType { get; set; }
    public ulong? ReferenceId { get; set; }

    // Audit fields
    public ulong? CreatedBy { get; set; }
    public ulong? ProcessedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual Order? Order { get; set; }
    public virtual User? Customer { get; set; }
    public virtual VendorProfile? Vendor { get; set; }
    public virtual Wallet? Wallet { get; set; }
    public virtual User? CreatedByNavigation { get; set; }
    public virtual User? ProcessedByNavigation { get; set; }
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual ICollection<Cashout> Cashouts { get; set; } = new List<Cashout>();
}

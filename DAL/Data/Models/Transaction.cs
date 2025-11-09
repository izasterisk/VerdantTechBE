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

    public ulong UserId { get; set; }

    // Status and metadata
    public TransactionStatus Status { get; set; } = TransactionStatus.Completed;

    [StringLength(255)]
    public string Note { get; set; } = null!;

    [StringLength(255)]
    public string? GatewayPaymentId { get; set; }

    // Audit fields
    public ulong? CreatedBy { get; set; }
    public ulong? ProcessedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual User? CreatedByNavigation { get; set; }
    public virtual User? ProcessedByNavigation { get; set; }
    public virtual ICollection<Cashout> Cashouts { get; set; } = new List<Cashout>();
}


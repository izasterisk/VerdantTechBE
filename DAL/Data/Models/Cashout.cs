using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Cashouts table (money going out - vendor payouts, expenses)
/// </summary>
public partial class Cashout
{
    public ulong Id { get; set; }

    public ulong? TransactionId { get; set; }

    public ulong? RecipientWalletId { get; set; }

    public decimal Amount { get; set; }

    public PayoutStatus Status { get; set; } = PayoutStatus.Pending;

    [StringLength(255)]
    public string? BankTransactionId { get; set; }

    /// <summary>
    /// Banking details (JSON)
    /// </summary>
    public Dictionary<string, object> BankingDetails { get; set; } = new();

    [StringLength(50)]
    public string? ReferenceType { get; set; }

    public ulong? ReferenceId { get; set; }

    public string? Notes { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual Transaction? Transaction { get; set; }
    public virtual Wallet? RecipientWallet { get; set; }
}

using DAL.Data;

namespace DAL.Data.Models;

/// <summary>
/// Transactions affecting wallet balances
/// </summary>
public partial class WalletTransaction
{
    public ulong Id { get; set; }

    public ulong WalletId { get; set; }

    public WalletTransactionType Type { get; set; }

    public decimal Amount { get; set; }

    public string? ReferenceType { get; set; }

    public ulong? ReferenceId { get; set; }

    public WalletTransactionStatus Status { get; set; } = WalletTransactionStatus.Pending;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation
    public virtual Wallet Wallet { get; set; } = null!;
}



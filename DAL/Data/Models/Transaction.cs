using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Central financial ledger - single source of truth for all money movements
/// </summary>
public partial class Transaction
{
    public ulong Id { get; set; }

    public TransactionType Type { get; set; }

    public decimal Amount { get; set; }

    [StringLength(255)]
    public string Description { get; set; } = null!;

    [StringLength(50)]
    public string? ReferenceType { get; set; }

    public ulong? ReferenceId { get; set; }

    public ulong? FromWalletId { get; set; }

    public ulong? ToWalletId { get; set; }

    public decimal BalanceBefore { get; set; } = 0.00m;

    public decimal BalanceAfter { get; set; } = 0.00m;

    public ulong CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation Properties
    public virtual Wallet? FromWallet { get; set; }
    public virtual Wallet? ToWallet { get; set; }
    public virtual User CreatedByNavigation { get; set; } = null!;
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual ICollection<Cashout> Cashouts { get; set; } = new List<Cashout>();
}

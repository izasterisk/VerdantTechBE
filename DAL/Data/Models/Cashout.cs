using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Money going out - vendor payouts and expenses with banking details
/// </summary>
public partial class Cashout
{
    public ulong Id { get; set; }

    public ulong UserId { get; set; }

    public ulong? TransactionId { get; set; }

    public ulong BankAccountId { get; set; }

    public decimal Amount { get; set; }

    public CashoutStatus Status { get; set; } = CashoutStatus.Processing;

    public CashoutReferenceType? ReferenceType { get; set; }

    public ulong? ReferenceId { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public ulong? ProcessedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual Transaction? Transaction { get; set; }
    public virtual UserBankAccount BankAccount { get; set; } = null!;
    public virtual User? ProcessedByNavigation { get; set; }
}

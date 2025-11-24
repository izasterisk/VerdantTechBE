using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Cashout-specific data - approval workflow and reference tracking
/// Core data (amount, status, user_id, bank_account_id) resides in transactions table
/// </summary>
public partial class Cashout
{
    public ulong Id { get; set; }

    // Reference to transaction (required - single source of truth)
    public ulong TransactionId { get; set; }

    public CashoutReferenceType ReferenceType { get; set; }

    public ulong? ReferenceId { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public ulong? ProcessedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual Transaction Transaction { get; set; } = null!;
    public virtual User? ProcessedByNavigation { get; set; }
}

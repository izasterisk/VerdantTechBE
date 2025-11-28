using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Bank accounts for all users (customers and vendors)
/// </summary>
public partial class UserBankAccount
{
    public ulong Id { get; set; }

    public ulong UserId { get; set; }

    [Required]
    [StringLength(20)]
    public string BankCode { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string AccountNumber { get; set; } = null!;

    [StringLength(50)]
    public string? OwnerName { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation
    public virtual User User { get; set; } = null!;
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}

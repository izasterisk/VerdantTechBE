namespace DAL.Data.Models;

/// <summary>
/// Vendor wallets tracking balances
/// </summary>
public partial class Wallet
{
    public ulong Id { get; set; }

    public ulong VendorId { get; set; }

    public decimal Balance { get; set; } = 0.00m;

    public decimal PendingWithdraw { get; set; } = 0.00m;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation
    public virtual VendorProfile VendorProfile { get; set; } = null!;
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}



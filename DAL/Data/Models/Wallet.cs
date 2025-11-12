namespace DAL.Data.Models;

/// <summary>
/// Vendor wallets tracking balances
/// </summary>
public partial class Wallet
{
    public ulong Id { get; set; }

    public ulong VendorId { get; set; }

    public decimal Balance { get; set; } = 0.00m;

    public ulong? LastUpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation
    public virtual User Vendor { get; set; } = null!;
    public virtual User? LastUpdatedByNavigation { get; set; }
}



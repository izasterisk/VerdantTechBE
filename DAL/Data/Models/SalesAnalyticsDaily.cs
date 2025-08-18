using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Daily sales analytics aggregation
/// </summary>
public partial class SalesAnalyticsDaily
{
    public ulong Id { get; set; }

    public DateOnly Date { get; set; }

    public ulong VendorId { get; set; }

    public int TotalOrders { get; set; } = 0;

    public decimal TotalRevenue { get; set; } = 0.00m;

    public int TotalProductsSold { get; set; } = 0;

    public decimal AverageOrderValue { get; set; } = 0.00m;

    public int NewCustomers { get; set; } = 0;

    public int ReturningCustomers { get; set; } = 0;

    /// <summary>
    /// Array of top selling products (JSON)
    /// </summary>
    public List<Dictionary<string, object>> TopProducts { get; set; } = new();

    public DateTime CreatedAt { get; set; }

    // Navigation Properties
    public virtual User Vendor { get; set; } = null!;
}

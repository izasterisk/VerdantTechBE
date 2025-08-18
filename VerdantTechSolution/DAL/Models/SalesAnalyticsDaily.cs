using System;
using System.Collections.Generic;

namespace DAL.Models;

/// <summary>
/// Daily sales analytics aggregation
/// </summary>
public partial class SalesAnalyticsDaily
{
    public ulong Id { get; set; }

    public DateOnly Date { get; set; }

    public ulong VendorId { get; set; }

    public int? TotalOrders { get; set; }

    public decimal? TotalRevenue { get; set; }

    public int? TotalProductsSold { get; set; }

    public decimal? AverageOrderValue { get; set; }

    public int? NewCustomers { get; set; }

    public int? ReturningCustomers { get; set; }

    /// <summary>
    /// Array of top selling products
    /// </summary>
    public string? TopProducts { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User Vendor { get; set; } = null!;
}

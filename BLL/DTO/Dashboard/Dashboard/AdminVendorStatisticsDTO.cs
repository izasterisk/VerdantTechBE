namespace BLL.DTO.Dashboard.Dashboard;

/// <summary>
/// Thống kê chi tiết về vendors
/// </summary>
public class AdminVendorStatisticsDTO
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public int TotalVendors { get; set; }
    public int VerifiedVendors { get; set; }
    public int PendingVerification { get; set; }
    public AdminVendorsByStatusDTO VendorsByStatus { get; set; } = new();
    public decimal TotalVendorRevenue { get; set; }
    public decimal TotalCommissionCollected { get; set; }
    public decimal AverageRevenuePerVendor { get; set; }
}

public class AdminVendorsByStatusDTO
{
    public int Active { get; set; }
    public int Inactive { get; set; }
    public int Suspended { get; set; }
}

/// <summary>
/// Top vendors có doanh thu cao nhất
/// </summary>
public class AdminTopVendorsDTO
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public List<AdminTopVendorItemDTO> Vendors { get; set; } = new();
}

public class AdminTopVendorItemDTO
{
    public int Rank { get; set; }
    public ulong VendorId { get; set; }
    public string CompanyName { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public DateTime? VerifiedAt { get; set; }
    public decimal GrossRevenue { get; set; }
    public decimal NetRevenue { get; set; }
    public decimal CommissionPaid { get; set; }
    public int OrderCount { get; set; }
    public int ProductCount { get; set; }
    public decimal AverageRating { get; set; }
    public decimal WalletBalance { get; set; }
}


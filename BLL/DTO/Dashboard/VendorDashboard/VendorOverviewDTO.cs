namespace BLL.DTO.Dashboard.VendorDashboard;

/// <summary>
/// Thống kê tổng quan nhanh cho vendor hiển thị trên dashboard home
/// </summary>
public class VendorOverviewDTO
{
    public decimal WalletBalance { get; set; }
    public decimal PendingCashout { get; set; }
    public decimal TotalRevenueThisMonth { get; set; }
    public decimal TotalRevenueLastMonth { get; set; }
    public decimal RevenueGrowthPercent { get; set; }
    public int TotalOrdersThisMonth { get; set; }
    public int TotalOrdersLastMonth { get; set; }
    public decimal OrderGrowthPercent { get; set; }
    public int TotalProductsActive { get; set; }
    public int TotalProductsOutOfStock { get; set; }
    public int PendingProductRegistrations { get; set; }
    public int PendingProductUpdateRequests { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
}


namespace BLL.DTO.Dashboard.Dashboard;

/// <summary>
/// Thống kê sản phẩm toàn hệ thống
/// </summary>
public class AdminProductStatisticsDTO
{
    public int TotalProducts { get; set; }
    public int ActiveProducts { get; set; }
    public int InactiveProducts { get; set; }
    public int OutOfStockProducts { get; set; }
    public int LowStockProducts { get; set; }
    public List<AdminCategoryDistributionDTO> CategoryDistribution { get; set; } = new();
    public AdminVendorProductDistributionDTO VendorDistribution { get; set; } = new();
}

public class AdminCategoryDistributionDTO
{
    public ulong CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public int ProductCount { get; set; }
    public decimal Percentage { get; set; }
    public int ActiveCount { get; set; }
    public int OutOfStockCount { get; set; }
}

public class AdminVendorProductDistributionDTO
{
    public int VendorsWithProducts { get; set; }
    public decimal AverageProductsPerVendor { get; set; }
    public int TopVendorProductCount { get; set; }
}

/// <summary>
/// Top sản phẩm bán chạy nhất toàn hệ thống
/// </summary>
public class AdminBestSellingProductsDTO
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public List<AdminBestSellingProductItemDTO> Products { get; set; } = new();
}

public class AdminBestSellingProductItemDTO
{
    public int Rank { get; set; }
    public ulong ProductId { get; set; }
    public string ProductCode { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public ulong VendorId { get; set; }
    public string VendorName { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int SoldQuantity { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal CommissionEarned { get; set; }
    public int StockQuantity { get; set; }
    public decimal RatingAverage { get; set; }
}


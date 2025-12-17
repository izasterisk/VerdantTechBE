namespace BLL.DTO.Dashboard.VendorDashboard;

/// <summary>
/// Thống kê sản phẩm của vendor
/// </summary>
public class VendorProductStatisticsDTO
{
    public int TotalProducts { get; set; }
    public int ActiveProducts { get; set; }
    public int InactiveProducts { get; set; }
    public int OutOfStockProducts { get; set; }
    public int LowStockProducts { get; set; }
    public int TotalStockQuantity { get; set; }
    public decimal TotalStockValue { get; set; }
    public List<VendorCategoryDistributionDTO> CategoryDistribution { get; set; } = new();
}

public class VendorCategoryDistributionDTO
{
    public ulong CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public int ProductCount { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>
/// Top sản phẩm bán chạy của vendor
/// </summary>
public class VendorBestSellingProductsDTO
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public List<VendorBestSellingProductItemDTO> Products { get; set; } = new();
}

public class VendorBestSellingProductItemDTO
{
    public int Rank { get; set; }
    public ulong ProductId { get; set; }
    public string ProductCode { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int SoldQuantity { get; set; }
    public decimal TotalRevenue { get; set; }
    public int StockQuantity { get; set; }
    public decimal RatingAverage { get; set; }
}

/// <summary>
/// Thống kê rating sản phẩm của vendor
/// </summary>
public class VendorProductRatingsDTO
{
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public VendorRatingDistributionDTO RatingDistribution { get; set; } = new();
    public List<VendorProductRatingItemDTO> Top3Highest { get; set; } = new();
    public List<VendorProductRatingItemDTO> Top3Lowest { get; set; } = new();
}

public class VendorRatingDistributionDTO
{
    public int Star5 { get; set; }
    public int Star4 { get; set; }
    public int Star3 { get; set; }
    public int Star2 { get; set; }
    public int Star1 { get; set; }
}

public class VendorProductRatingItemDTO
{
    public ulong ProductId { get; set; }
    public string ProductCode { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public decimal RatingAverage { get; set; }
    public int ReviewCount { get; set; }
}


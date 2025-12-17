namespace BLL.DTO.Dashboard.VendorDashboard;

/// <summary>
/// Doanh thu của vendor theo khoảng thời gian
/// </summary>
public class VendorRevenueDTO
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public decimal TotalGrossRevenue { get; set; }
    public decimal TotalCommission { get; set; }
    public decimal TotalNetRevenue { get; set; }
    public int TotalOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
}

/// <summary>
/// Doanh thu theo ngày cho biểu đồ
/// </summary>
public class VendorDailyRevenueDTO
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public List<VendorDailyRevenueItemDTO> DailyRevenues { get; set; } = new();
    public decimal TotalGrossRevenue { get; set; }
    public decimal TotalNetRevenue { get; set; }
    public int TotalOrders { get; set; }
}

public class VendorDailyRevenueItemDTO
{
    public DateOnly Date { get; set; }
    public decimal GrossRevenue { get; set; }
    public decimal NetRevenue { get; set; }
    public int OrderCount { get; set; }
}

/// <summary>
/// Doanh thu theo tháng trong năm
/// </summary>
public class VendorMonthlyRevenueDTO
{
    public int Year { get; set; }
    public List<VendorMonthlyRevenueItemDTO> MonthlyRevenues { get; set; } = new();
    public decimal TotalGrossRevenue { get; set; }
    public decimal TotalNetRevenue { get; set; }
    public int TotalOrders { get; set; }
}

public class VendorMonthlyRevenueItemDTO
{
    public int Month { get; set; }
    public string MonthName { get; set; } = null!;
    public decimal GrossRevenue { get; set; }
    public decimal NetRevenue { get; set; }
    public int OrderCount { get; set; }
}


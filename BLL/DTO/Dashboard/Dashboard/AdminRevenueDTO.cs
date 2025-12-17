namespace BLL.DTO.Dashboard.Dashboard;

/// <summary>
/// Doanh thu toàn hệ thống theo khoảng thời gian
/// </summary>
public class AdminRevenueDTO
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalCommission { get; set; }
    public decimal TotalVendorPayout { get; set; }
    public int TotalOrders { get; set; }
    public int TotalTransactions { get; set; }
    public decimal AverageOrderValue { get; set; }
    public AdminPaymentMethodBreakdownDTO PaymentMethodBreakdown { get; set; } = new();
}

public class AdminPaymentMethodBreakdownDTO
{
    public AdminPaymentMethodItemDTO Banking { get; set; } = new();
    public AdminPaymentMethodItemDTO Cod { get; set; } = new();
}

public class AdminPaymentMethodItemDTO
{
    public int Count { get; set; }
    public decimal Amount { get; set; }
}

/// <summary>
/// Doanh thu theo ngày cho biểu đồ
/// </summary>
public class AdminDailyRevenueDTO
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public List<AdminDailyRevenueItemDTO> DailyRevenues { get; set; } = new();
    public decimal TotalRevenue { get; set; }
    public decimal TotalCommission { get; set; }
    public int TotalOrders { get; set; }
}

public class AdminDailyRevenueItemDTO
{
    public DateOnly Date { get; set; }
    public decimal Revenue { get; set; }
    public decimal Commission { get; set; }
    public int OrderCount { get; set; }
    public int TransactionCount { get; set; }
}

/// <summary>
/// Doanh thu theo tháng trong năm
/// </summary>
public class AdminMonthlyRevenueDTO
{
    public int Year { get; set; }
    public List<AdminMonthlyRevenueItemDTO> MonthlyRevenues { get; set; } = new();
    public decimal TotalRevenue { get; set; }
    public decimal TotalCommission { get; set; }
    public int TotalOrders { get; set; }
}

public class AdminMonthlyRevenueItemDTO
{
    public int Month { get; set; }
    public string MonthName { get; set; } = null!;
    public decimal Revenue { get; set; }
    public decimal Commission { get; set; }
    public int OrderCount { get; set; }
}


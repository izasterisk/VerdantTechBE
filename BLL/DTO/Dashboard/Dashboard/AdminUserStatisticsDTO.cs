namespace BLL.DTO.Dashboard.Dashboard;

/// <summary>
/// Thống kê người dùng trên hệ thống
/// </summary>
public class AdminUserStatisticsDTO
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public AdminCustomerStatisticsDTO Customers { get; set; } = new();
    public AdminVendorUserStatisticsDTO Vendors { get; set; } = new();
    public AdminStaffStatisticsDTO Staff { get; set; } = new();
    public List<AdminRegistrationTrendDTO> RegistrationTrend { get; set; } = new();
}

public class AdminCustomerStatisticsDTO
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Inactive { get; set; }
    public int NewThisPeriod { get; set; }
    public decimal GrowthPercent { get; set; }
}

public class AdminVendorUserStatisticsDTO
{
    public int Total { get; set; }
    public int Verified { get; set; }
    public int PendingVerification { get; set; }
    public int NewThisPeriod { get; set; }
}

public class AdminStaffStatisticsDTO
{
    public int Total { get; set; }
    public int Active { get; set; }
}

public class AdminRegistrationTrendDTO
{
    public DateOnly Date { get; set; }
    public int Customers { get; set; }
    public int Vendors { get; set; }
}


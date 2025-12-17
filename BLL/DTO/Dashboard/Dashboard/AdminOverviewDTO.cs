namespace BLL.DTO.Dashboard.Dashboard;

/// <summary>
/// Thống kê tổng quan toàn hệ thống cho Admin/Staff
/// </summary>
public class AdminOverviewDTO
{
    public AdminRevenueOverviewDTO Revenue { get; set; } = new();
    public AdminCommissionOverviewDTO Commission { get; set; } = new();
    public AdminOrdersOverviewDTO Orders { get; set; } = new();
    public AdminUsersOverviewDTO Users { get; set; } = new();
    public AdminProductsOverviewDTO Products { get; set; } = new();
    public AdminPendingQueuesOverviewDTO PendingQueues { get; set; } = new();
}

public class AdminRevenueOverviewDTO
{
    public decimal Today { get; set; }
    public decimal ThisWeek { get; set; }
    public decimal ThisMonth { get; set; }
    public decimal LastMonth { get; set; }
    public decimal GrowthPercent { get; set; }
}

public class AdminCommissionOverviewDTO
{
    public decimal ThisMonth { get; set; }
    public decimal LastMonth { get; set; }
    public decimal GrowthPercent { get; set; }
}

public class AdminOrdersOverviewDTO
{
    public int Today { get; set; }
    public int ThisWeek { get; set; }
    public int ThisMonth { get; set; }
    public int PendingShipment { get; set; }
    public int InTransit { get; set; }
}

public class AdminUsersOverviewDTO
{
    public int TotalCustomers { get; set; }
    public int TotalVendors { get; set; }
    public int NewCustomersThisMonth { get; set; }
    public int NewVendorsThisMonth { get; set; }
}

public class AdminProductsOverviewDTO
{
    public int TotalActive { get; set; }
    public int TotalInactive { get; set; }
    public int OutOfStock { get; set; }
}

public class AdminPendingQueuesOverviewDTO
{
    public int VendorProfiles { get; set; }
    public int ProductRegistrations { get; set; }
    public int VendorCertificates { get; set; }
    public int ProductCertificates { get; set; }
    public int ProductUpdateRequests { get; set; }
    public int SupportRequests { get; set; }
    public int RefundRequests { get; set; }
    public int CashoutRequests { get; set; }
    public int Total => VendorProfiles + ProductRegistrations + VendorCertificates + ProductCertificates + 
                        ProductUpdateRequests + SupportRequests + RefundRequests + CashoutRequests;
}


namespace BLL.DTO.Dashboard.Dashboard;

/// <summary>
/// Chi tiết các hàng đợi yêu cầu chờ xử lý
/// </summary>
public class AdminQueueStatisticsDTO
{
    public AdminQueueItemDTO VendorProfiles { get; set; } = new();
    public AdminProductRegistrationQueueDTO ProductRegistrations { get; set; } = new();
    public AdminQueueItemDTO ProductUpdateRequests { get; set; } = new();
    public AdminQueueItemDTO VendorCertificates { get; set; } = new();
    public AdminQueueItemDTO ProductCertificates { get; set; } = new();
    public AdminRequestQueueDTO SupportRequests { get; set; } = new();
    public AdminRefundQueueDTO RefundRequests { get; set; } = new();
    public AdminCashoutQueueDTO CashoutRequests { get; set; } = new();
}

public class AdminQueueItemDTO
{
    public int PendingCount { get; set; }
    public DateTime? OldestPendingDate { get; set; }
    public decimal AverageWaitDays { get; set; }
}

public class AdminProductRegistrationQueueDTO : AdminQueueItemDTO
{
    public List<AdminQueueByVendorDTO> ByVendor { get; set; } = new();
}

public class AdminQueueByVendorDTO
{
    public ulong VendorId { get; set; }
    public string VendorName { get; set; } = null!;
    public int Count { get; set; }
}

public class AdminRequestQueueDTO : AdminQueueItemDTO
{
    public int InReviewCount { get; set; }
}

public class AdminRefundQueueDTO : AdminQueueItemDTO
{
    public int InReviewCount { get; set; }
    public decimal TotalPendingAmount { get; set; }
}

public class AdminCashoutQueueDTO : AdminQueueItemDTO
{
    public decimal TotalPendingAmount { get; set; }
}


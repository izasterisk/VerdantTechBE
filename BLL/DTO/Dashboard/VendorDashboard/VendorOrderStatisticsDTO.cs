namespace BLL.DTO.Dashboard.VendorDashboard;

/// <summary>
/// Thống kê đơn hàng của vendor theo trạng thái
/// </summary>
public class VendorOrderStatisticsDTO
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public int TotalOrders { get; set; }
    public VendorOrdersByStatusDTO OrdersByStatus { get; set; } = new();
    public decimal FulfillmentRate { get; set; }
    public decimal CancellationRate { get; set; }
    public decimal RefundRate { get; set; }
    public decimal AverageDeliveryDays { get; set; }
}

public class VendorOrdersByStatusDTO
{
    public int Pending { get; set; }
    public int Processing { get; set; }
    public int Paid { get; set; }
    public int Shipped { get; set; }
    public int Delivered { get; set; }
    public int Cancelled { get; set; }
    public int Refunded { get; set; }
    public int PartialRefund { get; set; }
}


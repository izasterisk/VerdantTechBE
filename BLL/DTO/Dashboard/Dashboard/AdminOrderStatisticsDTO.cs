namespace BLL.DTO.Dashboard.Dashboard;

/// <summary>
/// Thống kê đơn hàng toàn hệ thống
/// </summary>
public class AdminOrderStatisticsDTO
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public int TotalOrders { get; set; }
    public AdminOrdersByStatusDTO OrdersByStatus { get; set; } = new();
    public decimal FulfillmentRate { get; set; }
    public decimal CancellationRate { get; set; }
    public decimal RefundRate { get; set; }
    public decimal AverageDeliveryDays { get; set; }
    public AdminOrdersByPaymentMethodDTO OrdersByPaymentMethod { get; set; } = new();
    public List<AdminOrdersByCourierDTO> OrdersByCourier { get; set; } = new();
}

public class AdminOrdersByStatusDTO
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

public class AdminOrdersByPaymentMethodDTO
{
    public int Banking { get; set; }
    public int Cod { get; set; }
}

public class AdminOrdersByCourierDTO
{
    public int CourierId { get; set; }
    public string CourierName { get; set; } = null!;
    public int OrderCount { get; set; }
    public decimal Percentage { get; set; }
}


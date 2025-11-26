namespace BLL.DTO.Dashboard;

public class OrderStatisticsResponseDTO
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public int Total { get; set; }
    public int Paid { get; set; }
    public int Shipped { get; set; }
    public int Cancelled { get; set; }
    public int Delivered { get; set; }
    public int Refunded { get; set; }
}


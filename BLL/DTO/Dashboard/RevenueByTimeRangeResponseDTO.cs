namespace BLL.DTO.Dashboard;

public class RevenueByTimeRangeResponseDTO
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public decimal Revenue { get; set; }
}


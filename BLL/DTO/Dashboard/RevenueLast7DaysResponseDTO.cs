namespace BLL.DTO.Dashboard;

public class RevenueLast7DaysResponseDTO
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<DailyRevenueDTO> DailyRevenues { get; set; } = new();
}

public class DailyRevenueDTO
{
    public DateOnly Date { get; set; }
    public decimal Revenue { get; set; }
}


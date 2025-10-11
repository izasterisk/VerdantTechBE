namespace Infrastructure.Courier.Models;

public class DeliveryDate
{
    public int LeadTime { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}
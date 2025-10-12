namespace Infrastructure.Courier.Models;

public class CourierServices
{
    public int ServiceId { get; set; }
    public string ShortName { get; set; } = string.Empty;
    public int ServiceTypeId { get; set; }
}
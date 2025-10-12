namespace BLL.DTO.Courier;

public class CourierServicesResponseDTO
{
    public int ServiceId { get; set; }
    public string ShortName { get; set; } = string.Empty;
    public int ServiceTypeId { get; set; }
}
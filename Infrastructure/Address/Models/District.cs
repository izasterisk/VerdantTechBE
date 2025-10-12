namespace Infrastructure.Address.Models;

public class District
{
    public int DistrictId { get; set; }
    public int ProvinceId { get; set; }
    public string Name { get; set; } = string.Empty;
}
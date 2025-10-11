namespace Infrastructure.Address.Models;

public class District
{
    public int DistrictId { get; set; }
    public int ProvinceId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class DistrictsResponse
{
    public int Code { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<District> Data { get; set; } = new();
}
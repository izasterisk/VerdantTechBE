namespace Infrastructure.Address.Models;

public class District
{
    public string DistrictId { get; set; } = string.Empty;
    public string ProvinceId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class DistrictsResponse
{
    public int Code { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<District> Data { get; set; } = new();
}
namespace Infrastructure.Address.Models;

public class Province
{
    public string ProvinceId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class ProvincesResponse
{
    public int Code { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<Province> Data { get; set; } = new();
}
namespace Infrastructure.Address.Models;

public class Province
{
    public int ProvinceId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class ProvincesResponse
{
    public int Code { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<Province> Data { get; set; } = new();
}
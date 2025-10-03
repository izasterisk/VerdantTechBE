namespace Infrastructure.Courier.Models;

public class District
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CityId { get; set; } = string.Empty;
    public List<string> SupportCarriers { get; set; } = new();
}

public class DistrictsResponse
{
    public int Code { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<District> Data { get; set; } = new();
}
namespace Infrastructure.Courier.Models;

public class City
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> SupportCarriers { get; set; } = new();
}

public class CitiesResponse
{
    public int Code { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<City> Data { get; set; } = new();
}
namespace Infrastructure.Courier.Models;

public class Ward
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DistrictId { get; set; } = string.Empty;
    public List<string> SupportCarriers { get; set; } = new();
}

public class WardsResponse
{
    public int Code { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<Ward> Data { get; set; } = new();
}
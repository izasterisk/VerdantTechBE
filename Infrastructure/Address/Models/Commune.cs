namespace Infrastructure.Address.Models;

public class Commune
{
    public string CommuneCode { get; set; } = string.Empty;
    public int DistrictId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CommunesResponse
{
    public int Code { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<Commune> Data { get; set; } = new();
}
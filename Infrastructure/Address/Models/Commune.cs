namespace Infrastructure.Address.Models;

public class Commune
{
    public string CommuneCode { get; set; } = string.Empty;
    public int DistrictId { get; set; }
    public string Name { get; set; } = string.Empty;
}
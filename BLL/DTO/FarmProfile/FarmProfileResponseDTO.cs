using DAL.Data;

namespace BLL.DTO.FarmProfile;
public class FarmProfileResponseDTO
{
    public ulong Id { get; set; }
    public ulong UserId { get; set; }
    public string FarmName { get; set; } = null!;
    public decimal? FarmSizeHectares { get; set; }
    public string? LocationAddress { get; set; }
    public string? Province { get; set; }
    public string? District { get; set; }
    public string? Commune { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public FarmProfileStatus Status { get; set; }
    public string? PrimaryCrops { get; set; } 
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
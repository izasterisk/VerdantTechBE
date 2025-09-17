using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.FarmProfile;

public class FarmProfileUpdateDTO
{
    [StringLength(255)]
    public string? FarmName { get; set; }

    [Range(0, 100000, ErrorMessage = "FarmSizeHectares phải >= 0")]
    public decimal? FarmSizeHectares { get; set; }

    public string? LocationAddress { get; set; }

    [StringLength(100)]
    public string? Province { get; set; }

    [StringLength(100)]
    public string? District { get; set; }

    [StringLength(100)]
    public string? Commune { get; set; }

    // Lat/Lng WGS84
    [Range(-90, 90)]
    public decimal? Latitude { get; set; }

    [Range(-180, 180)]
    public decimal? Longitude { get; set; }
    
    [MinLength(0)]
    public List<string>? PrimaryCrops { get; set; }
}
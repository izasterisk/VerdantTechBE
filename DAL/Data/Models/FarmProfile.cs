using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Farm profile details for farmer users
/// </summary>
public partial class FarmProfile
{
    public ulong Id { get; set; }

    public ulong UserId { get; set; }

    [Required]
    [StringLength(255)]
    public string FarmName { get; set; } = null!;

    public decimal? FarmSizeHectares { get; set; }

    public string? LocationAddress { get; set; }

    [StringLength(100)]
    public string? Province { get; set; }

    [StringLength(100)]
    public string? District { get; set; }

    [StringLength(100)]
    public string? Commune { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    /// <summary>
    /// Array of main crops grown (JSON)
    /// </summary>
    public List<string> PrimaryCrops { get; set; } = new();

    public int? FarmingExperienceYears { get; set; }

    /// <summary>
    /// Array of certifications like organic, VietGAP, GlobalGAP (JSON)
    /// </summary>
    public List<string> CertificationTypes { get; set; } = new();

    [StringLength(100)]
    public string? SoilType { get; set; }

    [StringLength(100)]
    public string? IrrigationType { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<EnvironmentalDatum> EnvironmentalData { get; set; } = new List<EnvironmentalDatum>();
    public virtual ICollection<WeatherDataCache> WeatherDataCache { get; set; } = new List<WeatherDataCache>();
    public virtual ICollection<PlantDiseaseDetection> PlantDiseaseDetections { get; set; } = new List<PlantDiseaseDetection>();
}

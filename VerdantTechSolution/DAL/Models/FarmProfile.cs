using System;
using System.Collections.Generic;

namespace DAL.Models;

/// <summary>
/// Farm profile details for farmer users
/// </summary>
public partial class FarmProfile
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

    /// <summary>
    /// Array of main crops grown
    /// </summary>
    public string? PrimaryCrops { get; set; }

    public int? FarmingExperienceYears { get; set; }

    /// <summary>
    /// Array of certifications like organic, VietGAP, GlobalGAP
    /// </summary>
    public string? CertificationTypes { get; set; }

    public string? SoilType { get; set; }

    public string? IrrigationType { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<EnvironmentalDatum> EnvironmentalData { get; set; } = new List<EnvironmentalDatum>();

    public virtual ICollection<PlantDiseaseDetection> PlantDiseaseDetections { get; set; } = new List<PlantDiseaseDetection>();

    public virtual User User { get; set; } = null!;

    public virtual ICollection<WeatherDataCache> WeatherDataCaches { get; set; } = new List<WeatherDataCache>();
}

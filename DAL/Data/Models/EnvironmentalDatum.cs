using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Manual environmental data input by farmers
/// </summary>
public partial class EnvironmentalDatum
{
    public ulong Id { get; set; }

    public ulong FarmProfileId { get; set; }

    public ulong CustomerId { get; set; }

    public DateOnly MeasurementDate { get; set; }

    /// <summary>
    /// pH range 0-14
    /// </summary>
    public decimal? SoilPh { get; set; }

    /// <summary>
    /// CO2 emissions in kg
    /// </summary>
    public decimal? Co2Footprint { get; set; }

    public decimal? SoilMoisturePercentage { get; set; }

    public SoilType SoilType { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual FarmProfile FarmProfile { get; set; } = null!;
    public virtual User Customer { get; set; } = null!;
    public virtual ICollection<Fertilizer> Fertilizers { get; set; } = new List<Fertilizer>();
    public virtual ICollection<EnergyUsage> EnergyUsages { get; set; } = new List<EnergyUsage>();
}

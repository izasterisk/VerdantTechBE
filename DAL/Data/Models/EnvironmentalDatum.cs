using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Manual environmental data input by farmers
/// </summary>
public partial class EnvironmentalDatum
{
    public ulong Id { get; set; }

    public ulong FarmProfileId { get; set; }

    public ulong UserId { get; set; }

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

    /// <summary>
    /// N content in mg/kg
    /// </summary>
    public decimal? NitrogenLevel { get; set; }

    /// <summary>
    /// P content in mg/kg
    /// </summary>
    public decimal? PhosphorusLevel { get; set; }

    /// <summary>
    /// K content in mg/kg
    /// </summary>
    public decimal? PotassiumLevel { get; set; }

    public decimal? OrganicMatterPercentage { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual FarmProfile FarmProfile { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}

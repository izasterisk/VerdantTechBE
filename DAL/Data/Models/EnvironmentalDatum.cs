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

    /// <summary>
    /// Ngày bắt đầu ghi nhận dữ liệu
    /// </summary>
    public DateOnly MeasurementStartDate { get; set; }

    /// <summary>
    /// Ngày kết thúc ghi nhận dữ liệu
    /// </summary>
    public DateOnly MeasurementEndDate { get; set; }

    /// <summary>
    /// Sand (%) 0–30 cm
    /// </summary>
    public decimal? SandPct { get; set; }

    /// <summary>
    /// Silt (%) 0–30 cm
    /// </summary>
    public decimal? SiltPct { get; set; }

    /// <summary>
    /// Clay (%) 0–30 cm
    /// </summary>
    public decimal? ClayPct { get; set; }

    /// <summary>
    /// pH (H2O) 0–30 cm
    /// </summary>
    public decimal? Phh2o { get; set; }

    /// <summary>
    /// Độ ẩm đất (%) 0–30 cm
    /// </summary>
    public decimal? SoilMoisturePct { get; set; }

    /// <summary>
    /// Nhiệt độ đất (°C) 0–30 cm
    /// </summary>
    public decimal? SoilTemperatureC { get; set; }

    /// <summary>
    /// Tổng lượng mưa (mm)
    /// </summary>
    public decimal? PrecipitationSum { get; set; }

    /// <summary>
    /// ET0 FAO (mm)
    /// </summary>
    public decimal? Et0FaoEvapotranspiration { get; set; }

    /// <summary>
    /// CO2 emissions in kg
    /// </summary>
    public decimal? Co2Footprint { get; set; }

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

namespace BLL.DTO.CO2;

public class CO2FootprintResponseDTO
{
    // Environmental Data Properties
    public ulong Id { get; set; }
    public ulong FarmProfileId { get; set; }
    public ulong CustomerId { get; set; }
    public DateOnly MeasurementStartDate { get; set; }
    public DateOnly MeasurementEndDate { get; set; }
    public decimal? SandPct { get; set; }
    public decimal? SiltPct { get; set; }
    public decimal? ClayPct { get; set; }
    public decimal? Phh2o { get; set; }
    public decimal? PrecipitationSum { get; set; }
    public decimal? Et0FaoEvapotranspiration { get; set; }
    public decimal? Co2Footprint { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Energy Usage Properties
    public EnergyUsageDTO EnergyUsage { get; set; } = null!;

    // Fertilizer Properties
    public FertilizerDTO Fertilizer { get; set; } = null!;

}

public class EnergyUsageDTO
{
    public ulong Id { get; set; }
    public decimal ElectricityKwh { get; set; }
    public decimal GasolineLiters { get; set; }
    public decimal DieselLiters { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class FertilizerDTO
{
    public ulong Id { get; set; }
    public decimal OrganicFertilizer { get; set; }
    public decimal NpkFertilizer { get; set; }
    public decimal UreaFertilizer { get; set; }
    public decimal PhosphateFertilizer { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
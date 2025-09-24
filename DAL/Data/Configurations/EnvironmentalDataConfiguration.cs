using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAL.Data.Models;
using DAL.Data;

namespace DAL.Data.Configurations;

public class EnvironmentalDataConfiguration : IEntityTypeConfiguration<EnvironmentalDatum>
{
    public void Configure(EntityTypeBuilder<EnvironmentalDatum> builder)
    {
        builder.ToTable("environmental_data");
        
        // Primary Key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("bigint unsigned")
            .ValueGeneratedOnAdd();
        
        // Foreign Keys
        builder.Property(e => e.FarmProfileId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("farm_profile_id");
            
        builder.Property(e => e.CustomerId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("customer_id");
        
        // Date field
        builder.Property(e => e.MeasurementDate)
            .HasColumnType("date")
            .IsRequired()
            .HasColumnName("measurement_date")
            .HasComment("Ngày ghi nhận dữ liệu");

        // Soil composition (0–30 cm depth)
        builder.Property(e => e.SandPct)
            .HasPrecision(5, 2)
            .HasColumnName("sand_pct")
            .HasComment("Sand (%) 0–30 cm");

        builder.Property(e => e.SiltPct)
            .HasPrecision(5, 2)
            .HasColumnName("silt_pct")
            .HasComment("Silt (%) 0–30 cm");

        builder.Property(e => e.ClayPct)
            .HasPrecision(5, 2)
            .HasColumnName("clay_pct")
            .HasComment("Clay (%) 0–30 cm");

        // pH with updated precision and constraint
        builder.Property(e => e.Phh2o)
            .HasPrecision(4, 2)
            .HasColumnName("phh2o")
            .HasComment("pH (H2O) 0–30 cm");

        // Soil physical properties
        builder.Property(e => e.SoilMoisturePct)
            .HasPrecision(5, 2)
            .HasColumnName("soil_moisture_pct")
            .HasComment("Độ ẩm đất (%) 0–30 cm");

        builder.Property(e => e.SoilTemperatureC)
            .HasPrecision(5, 2)
            .HasColumnName("soil_temperature_c")
            .HasComment("Nhiệt độ đất (°C) 0–30 cm");

        // Hydrology data
        builder.Property(e => e.PrecipitationSum)
            .HasPrecision(7, 2)
            .HasColumnName("precipitation_sum")
            .HasComment("Tổng lượng mưa (mm)");

        builder.Property(e => e.Et0FaoEvapotranspiration)
            .HasPrecision(7, 2)
            .HasColumnName("et0_fao_evapotranspiration")
            .HasComment("ET0 FAO (mm)");

        // CO2 footprint
        builder.Property(e => e.Co2Footprint)
            .HasPrecision(10, 2)
            .HasColumnName("co2_footprint")
            .HasComment("Lượng khí thải CO2 tính bằng kg");
        
        // Text field
        builder.Property(e => e.Notes)
            .HasColumnType("text")
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci");
        
        // DateTime fields
        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at");
        
        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
            .HasColumnName("updated_at");
        
        // Foreign Key Relationships
        builder.HasOne(d => d.FarmProfile)
            .WithMany(p => p.EnvironmentalData)
            .HasForeignKey(d => d.FarmProfileId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(d => d.Customer)
            .WithMany(p => p.EnvironmentalDataAsCustomer)
            .HasForeignKey(d => d.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Indexes
        builder.HasIndex(e => new { e.FarmProfileId, e.MeasurementDate })
            .HasDatabaseName("idx_farm_date");
            
        builder.HasIndex(e => e.CustomerId)
            .HasDatabaseName("idx_customer");
    }
}

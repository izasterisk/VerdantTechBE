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
            
        builder.Property(e => e.UserId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("user_id");
        
        // Date field
        builder.Property(e => e.MeasurementDate)
            .HasColumnType("date")
            .IsRequired()
            .HasColumnName("measurement_date");
        
        // Decimal fields with precision
        builder.Property(e => e.SoilPh)
            .HasPrecision(3, 1)
            .HasColumnName("soil_ph");
            
        builder.Property(e => e.Co2Footprint)
            .HasPrecision(10, 2)
            .HasColumnName("co2_footprint");
            
        builder.Property(e => e.SoilMoisturePercentage)
            .HasPrecision(5, 2)
            .HasColumnName("soil_moisture_percentage");
            
        builder.Property(e => e.NitrogenLevel)
            .HasPrecision(10, 2)
            .HasColumnName("nitrogen_level");
            
        builder.Property(e => e.PhosphorusLevel)
            .HasPrecision(10, 2)
            .HasColumnName("phosphorus_level");
            
        builder.Property(e => e.PotassiumLevel)
            .HasPrecision(10, 2)
            .HasColumnName("potassium_level");
            
        builder.Property(e => e.OrganicMatterPercentage)
            .HasPrecision(5, 2)
            .HasColumnName("organic_matter_percentage");
        
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
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(d => d.User)
            .WithMany(p => p.EnvironmentalData)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indexes
        builder.HasIndex(e => new { e.FarmProfileId, e.MeasurementDate })
            .HasDatabaseName("idx_farm_date");
            
        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("idx_user");
            
        builder.HasIndex(e => e.MeasurementDate)
            .HasDatabaseName("idx_date");
    }
}

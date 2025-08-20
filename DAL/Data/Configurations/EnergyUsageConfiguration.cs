using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAL.Data.Models;

namespace DAL.Data.Configurations;

public class EnergyUsageConfiguration : IEntityTypeConfiguration<EnergyUsage>
{
    public void Configure(EntityTypeBuilder<EnergyUsage> builder)
    {
        builder.ToTable("energy_usage");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
            
        builder.Property(e => e.EnvironmentalDataId)
            .HasColumnName("environmental_data_id")
            .IsRequired();
            
        builder.Property(e => e.ElectricityKwh)
            .HasColumnName("electricity_kwh")
            .HasColumnType("decimal(10,2)")
            .HasDefaultValue(0.00m)
            .HasComment("Điện tiêu thụ (kWh)");
            
        builder.Property(e => e.GasolineLiters)
            .HasColumnName("gasoline_liters")
            .HasColumnType("decimal(10,2)")
            .HasDefaultValue(0.00m)
            .HasComment("Xăng sử dụng (lít)");
            
        builder.Property(e => e.DieselLiters)
            .HasColumnName("diesel_liters")
            .HasColumnType("decimal(10,2)")
            .HasDefaultValue(0.00m)
            .HasComment("Dầu diesel sử dụng (lít)");
            
        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
        
        // Foreign Key constraints
        builder.HasOne(e => e.EnvironmentalData)
            .WithMany(ed => ed.EnergyUsages)
            .HasForeignKey(e => e.EnvironmentalDataId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indexes
        builder.HasIndex(e => e.EnvironmentalDataId)
            .HasDatabaseName("idx_environmental_data");
    }
}

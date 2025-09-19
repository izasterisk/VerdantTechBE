using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAL.Data.Models;

namespace DAL.Data.Configurations;

public class FertilizerConfiguration : IEntityTypeConfiguration<Fertilizer>
{
    public void Configure(EntityTypeBuilder<Fertilizer> builder)
    {
        builder.ToTable("fertilizers");
        
        builder.HasKey(f => f.Id);
        
        builder.Property(f => f.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
            
        builder.Property(f => f.EnvironmentalDataId)
            .HasColumnName("environmental_data_id")
            .IsRequired();
            
        builder.Property(f => f.OrganicFertilizer)
            .HasColumnName("organic_fertilizer")
            .HasColumnType("decimal(10,2)")
            .HasDefaultValue(0.00m)
            .HasComment("Phân hữu cơ (kg)");
            
        builder.Property(f => f.NpkFertilizer)
            .HasColumnName("npk_fertilizer")
            .HasColumnType("decimal(10,2)")
            .HasDefaultValue(0.00m)
            .HasComment("Phân NPK tổng hợp (kg)");
            
        builder.Property(f => f.UreaFertilizer)
            .HasColumnName("urea_fertilizer")
            .HasColumnType("decimal(10,2)")
            .HasDefaultValue(0.00m)
            .HasComment("Phân urê (kg)");
            
        builder.Property(f => f.PhosphateFertilizer)
            .HasColumnName("phosphate_fertilizer")
            .HasColumnType("decimal(10,2)")
            .HasDefaultValue(0.00m)
            .HasComment("Phân lân (kg)");
            
        builder.Property(f => f.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
        builder.Property(f => f.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
        
        // Foreign Key constraints
        builder.HasOne(f => f.EnvironmentalData)
            .WithMany(e => e.Fertilizers)
            .HasForeignKey(f => f.EnvironmentalDataId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Indexes
        builder.HasIndex(f => f.EnvironmentalDataId)
            .HasDatabaseName("idx_environmental_data");
    }
}

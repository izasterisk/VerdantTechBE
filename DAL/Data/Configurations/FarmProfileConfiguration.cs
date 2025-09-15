using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using DAL.Data.Models;

namespace DAL.Data.Configurations;

public class FarmProfileConfiguration : IEntityTypeConfiguration<FarmProfile>
{
    public void Configure(EntityTypeBuilder<FarmProfile> builder)
    {
        builder.ToTable("farm_profiles");
        
        // Primary Key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("bigint unsigned")
            .ValueGeneratedOnAdd();
        
        // Foreign Key (unique)
        builder.Property(e => e.UserId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("user_id");
        
        // Required fields
        builder.Property(e => e.FarmName)
            .HasMaxLength(255)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("farm_name");
        
        // Optional fields
        builder.Property(e => e.FarmSizeHectares)
            .HasPrecision(10, 2)
            .HasColumnName("farm_size_hectares");
            
        builder.Property(e => e.LocationAddress)
            .HasColumnType("text")
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("location_address");
            
        builder.Property(e => e.Province)
            .HasMaxLength(100)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci");
            
        builder.Property(e => e.District)
            .HasMaxLength(100)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci");
            
        builder.Property(e => e.Commune)
            .HasMaxLength(100)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci");
        
        
        // Simple text fields
        builder.Property(e => e.PrimaryCrops)
            .HasMaxLength(500)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasComment("Main crops grown, comma-separated list")
            .HasColumnName("primary_crops");
        
        // Boolean fields
        builder.Property(e => e.IsActive)
            .HasDefaultValue(true)
            .HasColumnName("is_active");
        
        // DateTime fields
        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at");
        
        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
            .HasColumnName("updated_at");
        
        // Foreign Key Relationship
        builder.HasOne(d => d.User)
            .WithOne(p => p.FarmProfile)
            .HasForeignKey<FarmProfile>(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Unique constraint on UserId
        builder.HasIndex(e => e.UserId)
            .IsUnique()
            .HasDatabaseName("unique_user_id");
            
        // Indexes
        builder.HasIndex(e => new { e.Province, e.District })
            .HasDatabaseName("idx_location");
            
        builder.HasIndex(e => e.FarmSizeHectares)
            .HasDatabaseName("idx_farm_size");
            
        builder.HasIndex(e => e.IsActive)
            .HasDatabaseName("idx_active");
    }
}

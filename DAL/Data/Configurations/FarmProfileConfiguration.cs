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
        
        // Coordinates
        builder.Property(e => e.Latitude)
            .HasPrecision(10, 8)
            .HasComment("Farm latitude coordinate")
            .HasColumnName("latitude");
            
        builder.Property(e => e.Longitude)
            .HasPrecision(11, 8)
            .HasComment("Farm longitude coordinate")
            .HasColumnName("longitude");
        
        
        // Simple text fields
        builder.Property(e => e.PrimaryCrops)
            .HasMaxLength(500)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasComment("Main crops grown, comma-separated list")
            .HasColumnName("primary_crops");
        
        // Status enum field
        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasDefaultValue(FarmProfileStatus.Active)
            .HasColumnName("status");
        
        // DateTime fields
        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at");
        
        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
            .HasColumnName("updated_at");
        
        // Foreign Key Relationship (1 User -> Many FarmProfiles)
        builder.HasOne(d => d.User)
            .WithMany(p => p.FarmProfiles)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indexes (removed unique constraint on UserId)
        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("idx_user_id");
            
        // Indexes
        builder.HasIndex(e => new { e.Province, e.District })
            .HasDatabaseName("idx_location");
            
        builder.HasIndex(e => e.FarmSizeHectares)
            .HasDatabaseName("idx_farm_size");
            
        builder.HasIndex(e => e.Status)
            .HasDatabaseName("idx_status");
    }
}

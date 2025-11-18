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
            
        builder.Property(e => e.AddressId)
            .HasColumnType("bigint unsigned")
            .HasColumnName("address_id");
        
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
        
        // Foreign Key Relationships
        builder.HasOne(d => d.User)
            .WithMany(p => p.FarmProfiles)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(d => d.Address)
            .WithMany(p => p.FarmProfiles)
            .HasForeignKey(d => d.AddressId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Indexes
        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("idx_user");
            
        builder.HasIndex(e => e.AddressId)
            .HasDatabaseName("idx_address");
    }
}

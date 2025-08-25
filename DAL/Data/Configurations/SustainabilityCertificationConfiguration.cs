using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAL.Data.Models;

namespace DAL.Data.Configurations;

public class SustainabilityCertificationConfiguration : IEntityTypeConfiguration<SustainabilityCertification>
{
    public void Configure(EntityTypeBuilder<SustainabilityCertification> builder)
    {
        builder.ToTable("sustainability_certifications");
        
        // Primary Key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("bigint unsigned")
            .ValueGeneratedOnAdd();
        
        // Code field - unique
        builder.Property(e => e.Code)
            .HasMaxLength(100)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("code");
        
        // Name field
        builder.Property(e => e.Name)
            .HasMaxLength(255)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("name");
        
        // Category field - enum
        builder.Property(e => e.Category)
            .HasConversion<string>()
            .IsRequired()
            .HasColumnName("category");
        
        // Issuing body - nullable
        builder.Property(e => e.IssuingBody)
            .HasMaxLength(255)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("issuing_body");
        
        // Description field
        builder.Property(e => e.Description)
            .HasColumnType("text")
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("description");
        
        // Is active field
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
        
        // Unique constraints
        builder.HasIndex(e => e.Code)
            .IsUnique()
            .HasDatabaseName("idx_code");
        
        // Indexes
        builder.HasIndex(e => e.Category)
            .HasDatabaseName("idx_category");
            
        builder.HasIndex(e => e.IsActive)
            .HasDatabaseName("idx_active");
    }
}

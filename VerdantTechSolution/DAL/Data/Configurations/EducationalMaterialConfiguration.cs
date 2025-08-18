using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using DAL.Data.Models;
using DAL.Data;

namespace DAL.Data.Configurations;

public class EducationalMaterialConfiguration : IEntityTypeConfiguration<EducationalMaterial>
{
    public void Configure(EntityTypeBuilder<EducationalMaterial> builder)
    {
        builder.ToTable("educational_materials");
        
        // Primary Key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("bigint unsigned")
            .ValueGeneratedOnAdd();
        
        // Foreign Keys
        builder.Property(e => e.CreatedBy)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("created_by");
        
        // Enum conversions
        builder.Property(e => e.MaterialType)
            .HasConversion<string>()
            .HasColumnType("enum('guide','tutorial','research','case_study','infographic','video')")
            .IsRequired()
            .HasColumnName("material_type");
            
        builder.Property(e => e.Language)
            .HasConversion<string>()
            .HasColumnType("enum('vi','en')")
            .HasDefaultValue(Language.Vi);
            
        builder.Property(e => e.DifficultyLevel)
            .HasConversion<string>()
            .HasColumnType("enum('beginner','intermediate','advanced')")
            .HasDefaultValue(DifficultyLevel.Beginner)
            .HasColumnName("difficulty_level");
            
        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasColumnType("enum('draft','published','archived')")
            .HasDefaultValue(ContentStatus.Draft);
        
        // Required string fields
        builder.Property(e => e.Title)
            .HasMaxLength(255)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci");
            
        builder.Property(e => e.ContentUrl)
            .HasMaxLength(500)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("content_url");
        
        // Optional string fields
        builder.Property(e => e.Description)
            .HasColumnType("text")
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci");
            
        builder.Property(e => e.ThumbnailUrl)
            .HasMaxLength(500)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("thumbnail_url");
        
        // Decimal field
        builder.Property(e => e.FileSizeMb)
            .HasPrecision(10, 2)
            .HasColumnName("file_size_mb");
            
        builder.Property(e => e.RatingAverage)
            .HasPrecision(3, 2)
            .HasDefaultValue(0.00m)
            .HasColumnName("rating_average");
        
        // Integer fields
        builder.Property(e => e.DurationMinutes)
            .HasColumnName("duration_minutes");
            
        builder.Property(e => e.TotalRatings)
            .HasDefaultValue(0)
            .HasColumnName("total_ratings");
        
        // Long count fields with defaults
        builder.Property(e => e.DownloadCount)
            .HasDefaultValue(0L)
            .HasColumnName("download_count");
            
        builder.Property(e => e.ViewCount)
            .HasDefaultValue(0L)
            .HasColumnName("view_count");
        
        // Boolean defaults
        builder.Property(e => e.IsPremium)
            .HasDefaultValue(false)
            .HasColumnName("is_premium");
        
        // JSON fields for arrays
        builder.Property(e => e.Topics)
            .HasConversion(
                v => v == null || v.Count == 0 ? "[]" : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v) || v == "[]" ? new List<string>() : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("json");
            
        builder.Property(e => e.TargetAudience)
            .HasConversion(
                v => v == null || v.Count == 0 ? "[]" : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v) || v == "[]" ? new List<string>() : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("json")
            .HasColumnName("target_audience");
        
        // DateTime fields
        builder.Property(e => e.PublishedAt)
            .HasColumnType("timestamp")
            .HasColumnName("published_at");
            
        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at");
        
        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
            .HasColumnName("updated_at");
        
        // Foreign Key Relationships
        builder.HasOne(d => d.CreatedByNavigation)
            .WithMany(p => p.EducationalMaterials)
            .HasForeignKey(d => d.CreatedBy)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indexes
        builder.HasIndex(e => e.CreatedBy)
            .HasDatabaseName("idx_creator");
            
        builder.HasIndex(e => e.MaterialType)
            .HasDatabaseName("idx_type");
            
        builder.HasIndex(e => e.Language)
            .HasDatabaseName("idx_language");
            
        builder.HasIndex(e => e.Status)
            .HasDatabaseName("idx_status");
            
        builder.HasIndex(e => e.RatingAverage)
            .HasDatabaseName("idx_rating");
        
        // Full-text search index
        builder.HasIndex(e => new { e.Title, e.Description })
            .HasAnnotation("MySql:FullTextIndex", true)
            .HasDatabaseName("idx_search");
    }
}

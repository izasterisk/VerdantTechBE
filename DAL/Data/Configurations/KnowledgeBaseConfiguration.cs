using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using DAL.Data.Models;
using DAL.Data;

namespace DAL.Data.Configurations;

public class KnowledgeBaseConfiguration : IEntityTypeConfiguration<KnowledgeBase>
{
    public void Configure(EntityTypeBuilder<KnowledgeBase> builder)
    {
        builder.ToTable("knowledge_base");
        
        // Primary Key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("bigint unsigned")
            .ValueGeneratedOnAdd();
        
        // Foreign Keys
        builder.Property(e => e.VerifiedBy)
            .HasColumnType("bigint unsigned")
            .HasColumnName("verified_by");
            
        builder.Property(e => e.CreatedBy)
            .HasColumnType("bigint unsigned")
            .HasColumnName("created_by");
        
        // Required string fields
        builder.Property(e => e.Category)
            .HasMaxLength(100)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci");
            
        builder.Property(e => e.Question)
            .HasColumnType("text")
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci");
            
        builder.Property(e => e.Answer)
            .HasColumnType("text")
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci");
        
        // Optional string fields
        builder.Property(e => e.Subcategory)
            .HasMaxLength(100)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci");
            
        builder.Property(e => e.SourceUrl)
            .HasMaxLength(500)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("source_url");
        
        // JSON field for keywords
        builder.Property(e => e.Keywords)
            .HasConversion(
                v => v == null || v.Count == 0 ? "[]" : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v) || v == "[]" ? new List<string>() : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("json");
        
        // Enum conversion for language
        builder.Property(e => e.Language)
            .HasConversion<string>()
            .HasColumnType("enum('vi','en')")
            .HasDefaultValue(Language.Vi);
        
        // Boolean defaults
        builder.Property(e => e.IsVerified)
            .HasDefaultValue(false)
            .HasColumnName("is_verified");
        
        // Count fields with defaults
        builder.Property(e => e.UsageCount)
            .HasDefaultValue(0L)
            .HasColumnName("usage_count");
            
        builder.Property(e => e.HelpfulCount)
            .HasDefaultValue(0)
            .HasColumnName("helpful_count");
            
        builder.Property(e => e.UnhelpfulCount)
            .HasDefaultValue(0)
            .HasColumnName("unhelpful_count");
        
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
        builder.HasOne(d => d.VerifiedByNavigation)
            .WithMany(p => p.VerifiedKnowledgeBaseEntries)
            .HasForeignKey(d => d.VerifiedBy)
            .OnDelete(DeleteBehavior.SetNull);
            
        builder.HasOne(d => d.CreatedByNavigation)
            .WithMany(p => p.KnowledgeBaseEntries)
            .HasForeignKey(d => d.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull);
        
        // Indexes
        builder.HasIndex(e => new { e.Category, e.Subcategory })
            .HasDatabaseName("idx_category");
            
        builder.HasIndex(e => e.Language)
            .HasDatabaseName("idx_language");
            
        builder.HasIndex(e => e.IsVerified)
            .HasDatabaseName("idx_verified");
        
        // Full-text search index
        builder.HasIndex(e => new { e.Question, e.Answer })
            .HasAnnotation("MySql:FullTextIndex", true)
            .HasDatabaseName("idx_search");
    }
}

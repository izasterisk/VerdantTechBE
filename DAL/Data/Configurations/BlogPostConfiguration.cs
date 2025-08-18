using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using DAL.Data.Models;
using DAL.Data;

namespace DAL.Data.Configurations;

public class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
{
    public void Configure(EntityTypeBuilder<BlogPost> builder)
    {
        builder.ToTable("blog_posts");
        
        // Primary Key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("bigint unsigned")
            .ValueGeneratedOnAdd();
        
        // Foreign Keys
        builder.Property(e => e.AuthorId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("author_id");
        
        // Required string fields
        builder.Property(e => e.Category)
            .HasMaxLength(100)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci");
            
        builder.Property(e => e.Title)
            .HasMaxLength(255)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci");
            
        builder.Property(e => e.Slug)
            .HasMaxLength(255)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci");
            
        builder.Property(e => e.Content)
            .HasColumnType("text")
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci");
        
        // Optional text fields
        builder.Property(e => e.Excerpt)
            .HasColumnType("text")
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci");
            
        builder.Property(e => e.FeaturedImageUrl)
            .HasMaxLength(500)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("featured_image_url");
            
        builder.Property(e => e.SeoTitle)
            .HasMaxLength(255)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("seo_title");
            
        builder.Property(e => e.SeoDescription)
            .HasColumnType("text")
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("seo_description");
        
        // JSON fields for arrays
        builder.Property(e => e.Tags)
            .HasConversion(
                v => v == null || v.Count == 0 ? "[]" : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v) || v == "[]" ? new List<string>() : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("json");
            
        builder.Property(e => e.SeoKeywords)
            .HasConversion(
                v => v == null || v.Count == 0 ? "[]" : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v) || v == "[]" ? new List<string>() : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("json")
            .HasColumnName("seo_keywords");
        
        // Count fields with defaults
        builder.Property(e => e.ViewCount)
            .HasDefaultValue(0L)
            .HasColumnName("view_count");
            
        builder.Property(e => e.CommentCount)
            .HasDefaultValue(0)
            .HasColumnName("comment_count");
            
        builder.Property(e => e.LikeCount)
            .HasDefaultValue(0)
            .HasColumnName("like_count");
            
        builder.Property(e => e.DislikeCount)
            .HasDefaultValue(0)
            .HasColumnName("dislike_count");
            
        builder.Property(e => e.ReadingTimeMinutes)
            .HasColumnName("reading_time_minutes");
        
        // Boolean defaults
        builder.Property(e => e.IsFeatured)
            .HasDefaultValue(false)
            .HasColumnName("is_featured");
        
        // Enum conversion for status
        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasColumnType("enum('draft','published','scheduled','archived')")
            .HasDefaultValue(BlogStatus.Draft);
        
        // DateTime fields
        builder.Property(e => e.PublishedAt)
            .HasColumnType("timestamp")
            .HasColumnName("published_at");
            
        builder.Property(e => e.ScheduledAt)
            .HasColumnType("timestamp")
            .HasColumnName("scheduled_at");
            
        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at");
        
        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
            .HasColumnName("updated_at");
        
        // Foreign Key Relationships
        builder.HasOne(d => d.Author)
            .WithMany(p => p.BlogPosts)
            .HasForeignKey(d => d.AuthorId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Unique constraint
        builder.HasIndex(e => e.Slug)
            .IsUnique()
            .HasDatabaseName("idx_slug");
        
        // Indexes
        builder.HasIndex(e => e.AuthorId)
            .HasDatabaseName("idx_author");
            
        builder.HasIndex(e => new { e.Status, e.IsFeatured })
            .HasDatabaseName("idx_status_featured");
            
        builder.HasIndex(e => e.PublishedAt)
            .HasDatabaseName("idx_published");
            
        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("idx_created_at");
        
        // Full-text search index
        builder.HasIndex(e => new { e.Title, e.Excerpt, e.Content })
            .HasAnnotation("MySql:FullTextIndex", true)
            .HasDatabaseName("idx_search");
    }
}

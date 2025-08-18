using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using DAL.Data.Models;
using DAL.Data;

namespace DAL.Data.Configurations;

public class ForumPostConfiguration : IEntityTypeConfiguration<ForumPost>
{
    public void Configure(EntityTypeBuilder<ForumPost> builder)
    {
        builder.ToTable("forum_posts");
        
        // Primary Key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("bigint unsigned")
            .ValueGeneratedOnAdd();
        
        // Foreign Keys
        builder.Property(e => e.CategoryId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("category_id");
            
        builder.Property(e => e.UserId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("user_id");
            
        builder.Property(e => e.ModeratedBy)
            .HasColumnType("bigint unsigned")
            .HasColumnName("moderated_by");
        
        // Required string fields
        builder.Property(e => e.Title)
            .HasMaxLength(255)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci");
            
        builder.Property(e => e.Content)
            .HasColumnType("text")
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci");
        
        // Optional text field
        builder.Property(e => e.ModeratedReason)
            .HasColumnType("text")
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("moderated_reason");
        
        // JSON field for tags array
        builder.Property(e => e.Tags)
            .HasConversion(
                v => v == null || v.Count == 0 ? "[]" : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v) || v == "[]" ? new List<string>() : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("json");
        
        // Count fields with defaults
        builder.Property(e => e.ViewCount)
            .HasDefaultValue(0L)
            .HasColumnName("view_count");
            
        builder.Property(e => e.ReplyCount)
            .HasDefaultValue(0)
            .HasColumnName("reply_count");
            
        builder.Property(e => e.LikeCount)
            .HasDefaultValue(0)
            .HasColumnName("like_count");
            
        builder.Property(e => e.DislikeCount)
            .HasDefaultValue(0)
            .HasColumnName("dislike_count");
        
        // Boolean defaults
        builder.Property(e => e.IsPinned)
            .HasDefaultValue(false)
            .HasColumnName("is_pinned");
            
        builder.Property(e => e.IsLocked)
            .HasDefaultValue(false)
            .HasColumnName("is_locked");
        
        // Enum conversion for status
        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasColumnType("enum('published','draft','moderated','deleted')")
            .HasDefaultValue(ForumPostStatus.Published);
        
        // DateTime fields
        builder.Property(e => e.LastActivityAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("last_activity_at");
            
        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at");
        
        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
            .HasColumnName("updated_at");
        
        // Foreign Key Relationships
        
        // Relationship with ForumCategory
        builder.HasOne(d => d.Category)
            .WithMany(p => p.ForumPosts)
            .HasForeignKey(d => d.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Relationship with User (post author)
        builder.HasOne(d => d.User)
            .WithMany(p => p.ForumPosts)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Relationship with User (moderator)
        builder.HasOne(d => d.ModeratedByNavigation)
            .WithMany(p => p.ModeratedForumPosts)
            .HasForeignKey(d => d.ModeratedBy)
            .OnDelete(DeleteBehavior.SetNull);
        
        // Indexes
        builder.HasIndex(e => e.CategoryId)
            .HasDatabaseName("idx_category");
            
        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("idx_user");
            
        builder.HasIndex(e => new { e.Status, e.IsPinned })
            .HasDatabaseName("idx_status_pinned");
            
        builder.HasIndex(e => e.LastActivityAt)
            .HasDatabaseName("idx_last_activity");
            
        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("idx_created_at");
        
        // Full-text search index
        builder.HasIndex(e => new { e.Title, e.Content })
            .HasAnnotation("MySql:FullTextIndex", true)
            .HasDatabaseName("idx_search");
    }
}

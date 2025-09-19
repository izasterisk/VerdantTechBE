using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using DAL.Data.Models;

namespace DAL.Data.Configurations;

// var post = new ForumPost 
// {
//     Title = "Hướng dẫn trồng rau",
//     Content = new List<ContentBlock>
//     {
//         new ContentBlock { Order = 1, Type = "text", Content = "Giới thiệu về..." },
//         new ContentBlock { Order = 2, Type = "image", Content = "https://example.com/img.jpg" },
//         new ContentBlock { Order = 3, Type = "text", Content = "Bước tiếp theo..." }
//     },
//     Tags = new List<string> { "nông nghiệp", "rau sạch" }
// };

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
        builder.Property(e => e.ForumCategoryId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("forum_category_id");

        builder.Property(e => e.UserId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("user_id");


        // Required string fields
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

        // JSON field for mixed content blocks using JsonHelpers
        builder.Property(e => e.Content)
            .HasConversion(JsonHelpers.ListContentBlockConverter())
            .HasColumnType("json")
            .IsRequired()
            .Metadata.SetValueComparer(JsonHelpers.ListContentBlockComparer());

        // VARCHAR field for tags array
        builder.Property(e => e.Tags)
            .HasMaxLength(1000)
            .IsRequired(false);


        // Count fields with defaults
        builder.Property(e => e.ViewCount)
            .HasDefaultValue(0L)
            .HasColumnName("view_count");


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


        // Enum conversion for status
        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasColumnType("enum('visible','hidden')")
            .HasDefaultValue(ForumPostStatus.Visible);

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

        // Relationship with ForumCategory
        builder.HasOne(d => d.ForumCategory)
            .WithMany(p => p.ForumPosts)
            .HasForeignKey(d => d.ForumCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship with User (post author)
        builder.HasOne(d => d.User)
            .WithMany(p => p.ForumPosts)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Restrict);


        // Indexes
        builder.HasIndex(e => e.ForumCategoryId)
            .HasDatabaseName("idx_category");

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("idx_user");
            
        builder.HasIndex(e => e.Slug)
            .IsUnique()
            .HasDatabaseName("idx_slug");

        builder.HasIndex(e => new { e.Status, e.IsPinned })
            .HasDatabaseName("idx_status_pinned");


        // Full-text search index - chỉ search title theo schema mới
        builder.HasIndex(e => e.Title)
            .HasAnnotation("MySql:FullTextIndex", true)
            .HasDatabaseName("idx_search");
    }
}

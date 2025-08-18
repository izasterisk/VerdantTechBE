using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAL.Data.Models;
using DAL.Data;

namespace DAL.Data.Configurations;

public class BlogCommentConfiguration : IEntityTypeConfiguration<BlogComment>
{
    public void Configure(EntityTypeBuilder<BlogComment> builder)
    {
        builder.ToTable("blog_comments");
        
        // Primary Key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("bigint unsigned")
            .ValueGeneratedOnAdd();
        
        // Foreign Keys
        builder.Property(e => e.PostId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("post_id");
            
        builder.Property(e => e.UserId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("user_id");
            
        builder.Property(e => e.ParentId)
            .HasColumnType("bigint unsigned")
            .HasColumnName("parent_id");
            
        builder.Property(e => e.ModeratedBy)
            .HasColumnType("bigint unsigned")
            .HasColumnName("moderated_by");
        
        // Required Content field
        builder.Property(e => e.Content)
            .HasColumnType("text")
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci");
        
        // Count fields with defaults
        builder.Property(e => e.LikeCount)
            .HasDefaultValue(0)
            .HasColumnName("like_count");
            
        builder.Property(e => e.DislikeCount)
            .HasDefaultValue(0)
            .HasColumnName("dislike_count");
        
        // Enum conversion for status
        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasColumnType("enum('approved','pending','spam','deleted')")
            .HasDefaultValue(BlogCommentStatus.Pending);
        
        // DateTime fields
        builder.Property(e => e.ModeratedAt)
            .HasColumnType("timestamp")
            .HasColumnName("moderated_at");
            
        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at");
        
        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
            .HasColumnName("updated_at");
        
        // Foreign Key Relationships
        
        // Relationship with BlogPost
        builder.HasOne(d => d.Post)
            .WithMany(p => p.BlogComments)
            .HasForeignKey(d => d.PostId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Relationship with User (commenter)
        builder.HasOne(d => d.User)
            .WithMany(p => p.BlogComments)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Relationship with User (moderator) - this is the problematic one
        builder.HasOne(d => d.ModeratedByNavigation)
            .WithMany(p => p.ModeratedBlogComments)
            .HasForeignKey(d => d.ModeratedBy)
            .OnDelete(DeleteBehavior.SetNull);
        
        // Self-referencing relationship (Parent-Child comments)
        builder.HasOne(d => d.Parent)
            .WithMany(p => p.InverseParent)
            .HasForeignKey(d => d.ParentId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indexes
        builder.HasIndex(e => new { e.PostId, e.Status })
            .HasDatabaseName("idx_post_status");
            
        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("idx_user");
            
        builder.HasIndex(e => e.ParentId)
            .HasDatabaseName("idx_parent");
            
        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("idx_created_at");
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAL.Data.Models;
using DAL.Data;

namespace DAL.Data.Configurations;

public class ForumCommentConfiguration : IEntityTypeConfiguration<ForumComment>
{
    public void Configure(EntityTypeBuilder<ForumComment> builder)
    {
        builder.ToTable("forum_comments");
        
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
        
        // Optional text field
        builder.Property(e => e.ModeratedReason)
            .HasColumnType("text")
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("moderated_reason");
        
        // Count fields with defaults
        builder.Property(e => e.LikeCount)
            .HasDefaultValue(0)
            .HasColumnName("like_count");
            
        builder.Property(e => e.DislikeCount)
            .HasDefaultValue(0)
            .HasColumnName("dislike_count");
        
        // Boolean defaults
        builder.Property(e => e.IsSolution)
            .HasDefaultValue(false)
            .HasColumnName("is_solution");
        
        // Enum conversion for status
        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasColumnType("enum('visible','moderated','deleted')")
            .HasDefaultValue(ForumCommentStatus.Visible);
        
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
        
        // Relationship with ForumPost
        builder.HasOne(d => d.Post)
            .WithMany(p => p.ForumComments)
            .HasForeignKey(d => d.PostId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Relationship with User (commenter)
        builder.HasOne(d => d.User)
            .WithMany(p => p.ForumComments)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Self-referencing relationship (Parent-Child comments)
        builder.HasOne(d => d.Parent)
            .WithMany(p => p.InverseParent)
            .HasForeignKey(d => d.ParentId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Relationship with User (moderator)
        builder.HasOne(d => d.ModeratedByNavigation)
            .WithMany(p => p.ModeratedForumComments)
            .HasForeignKey(d => d.ModeratedBy)
            .OnDelete(DeleteBehavior.SetNull);
        
        // Indexes
        builder.HasIndex(e => e.PostId)
            .HasDatabaseName("idx_post");
            
        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("idx_user");
            
        builder.HasIndex(e => e.ParentId)
            .HasDatabaseName("idx_parent");
            
        builder.HasIndex(e => e.Status)
            .HasDatabaseName("idx_status");
            
        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("idx_created_at");
    }
}

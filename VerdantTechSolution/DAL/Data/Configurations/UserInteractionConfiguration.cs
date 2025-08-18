using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAL.Data.Models;
using DAL.Data;

namespace DAL.Data.Configurations;

public class UserInteractionConfiguration : IEntityTypeConfiguration<UserInteraction>
{
    public void Configure(EntityTypeBuilder<UserInteraction> builder)
    {
        builder.ToTable("user_interactions");
        
        // Primary Key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("bigint unsigned")
            .ValueGeneratedOnAdd();
        
        // Foreign Keys
        builder.Property(e => e.UserId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("user_id");
            
        builder.Property(e => e.TargetId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("target_id");
        
        // Enum conversions
        builder.Property(e => e.TargetType)
            .HasConversion<string>()
            .HasColumnType("enum('forum_post','forum_comment','blog_post','blog_comment','product_review')")
            .IsRequired()
            .HasColumnName("target_type");
            
        builder.Property(e => e.InteractionType)
            .HasConversion<string>()
            .HasColumnType("enum('like','dislike','helpful','unhelpful')")
            .IsRequired()
            .HasColumnName("interaction_type");
        
        // DateTime field
        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at");
        
        // Foreign Key Relationships
        builder.HasOne(d => d.User)
            .WithMany(p => p.UserInteractions)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Unique constraint - one interaction per user per target per type
        builder.HasIndex(e => new { e.UserId, e.TargetType, e.TargetId, e.InteractionType })
            .IsUnique()
            .HasDatabaseName("unique_user_target_interaction");
        
        // Indexes
        builder.HasIndex(e => new { e.TargetType, e.TargetId })
            .HasDatabaseName("idx_target");
            
        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("idx_user");
    }
}

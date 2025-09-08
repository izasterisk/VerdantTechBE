using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using DAL.Data.Models;
using DAL.Data;

namespace DAL.Data.Configurations;

public class ChatbotConversationConfiguration : IEntityTypeConfiguration<ChatbotConversation>
{
    public void Configure(EntityTypeBuilder<ChatbotConversation> builder)
    {
        builder.ToTable("chatbot_conversations");
        
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
        
        // Required string fields
        builder.Property(e => e.SessionId)
            .HasMaxLength(255)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("session_id");
        
        // Optional string field
        builder.Property(e => e.Title)
            .HasMaxLength(255)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci");
        
        // TEXT field for context
        builder.Property(e => e.Context)
            .HasColumnType("text")
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .IsRequired(false);
        
        // Boolean default
        builder.Property(e => e.IsActive)
            .HasDefaultValue(true)
            .HasColumnName("is_active");
        
        // DateTime fields
        builder.Property(e => e.StartedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("started_at");
            
        builder.Property(e => e.EndedAt)
            .HasColumnType("timestamp")
            .HasColumnName("ended_at");
        
        // Foreign Key Relationships
        builder.HasOne(d => d.User)
            .WithMany(p => p.ChatbotConversations)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indexes
        builder.HasIndex(e => new { e.UserId, e.SessionId })
            .HasDatabaseName("idx_user_session");
            
        builder.HasIndex(e => e.IsActive)
            .HasDatabaseName("idx_active");
            
        builder.HasIndex(e => e.StartedAt)
            .HasDatabaseName("idx_started");
    }
}

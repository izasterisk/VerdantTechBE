using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAL.Data.Models;

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
        builder.Property(e => e.CustomerId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("customer_id");
        
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
        builder.HasOne(d => d.Customer)
            .WithMany(p => p.ChatbotConversations)
            .HasForeignKey(d => d.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Indexes
        builder.HasIndex(e => e.CustomerId)
            .HasDatabaseName("idx_customer");
            
        builder.HasIndex(e => e.SessionId)
            .HasDatabaseName("idx_session");
    }
}

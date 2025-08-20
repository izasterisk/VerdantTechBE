using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAL.Data.Models;
using System.Text.Json;

namespace DAL.Data.Configurations;

public class ChatbotMessageConfiguration : IEntityTypeConfiguration<ChatbotMessage>
{
    public void Configure(EntityTypeBuilder<ChatbotMessage> builder)
    {
        // Primary Key
        builder.HasKey(e => e.Id);

        // Table configuration
        builder.ToTable("chatbot_messages");

        // Property configurations
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(e => e.ConversationId)
            .HasColumnName("conversation_id")
            .HasColumnType("bigint unsigned")
            .IsRequired();

        builder.Property(e => e.MessageType)
            .HasColumnName("message_type")
            .HasConversion<string>()
            .HasColumnType("enum('user','bot','system')")
            .IsRequired();

        builder.Property(e => e.MessageText)
            .HasColumnName("message_text")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(e => e.Attachments)
            .HasColumnName("attachments")
            .HasColumnType("json")
            .HasConversion(
                v => v == null ? "[]" : JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => string.IsNullOrEmpty(v) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null!)!
            );

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Foreign Key relationships
        builder.HasOne(e => e.Conversation)
            .WithMany()
            .HasForeignKey(e => e.ConversationId)
            .HasConstraintName("fk_chatbot_messages_conversation_id")
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => e.ConversationId)
            .HasDatabaseName("idx_conversation");

        builder.HasIndex(e => e.MessageType)
            .HasDatabaseName("idx_type");

        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("idx_created");
    }
}

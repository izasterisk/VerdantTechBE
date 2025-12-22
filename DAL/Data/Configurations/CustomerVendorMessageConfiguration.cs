using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAL.Data.Models;

namespace DAL.Data.Configurations;

public class CustomerVendorMessageConfiguration : IEntityTypeConfiguration<CustomerVendorMessage>
{
    public void Configure(EntityTypeBuilder<CustomerVendorMessage> builder)
    {
        // Primary Key
        builder.HasKey(e => e.Id);

        // Table configuration
        builder.ToTable("customer_vendor_messages");

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

        builder.Property(e => e.SenderType)
            .HasColumnName("sender_type")
            .HasConversion<string>()
            .HasColumnType("enum('customer','vendor')")
            .IsRequired();

        builder.Property(e => e.MessageText)
            .HasColumnName("message_text")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(e => e.IsRead)
            .HasColumnName("is_read")
            .HasColumnType("tinyint(1)")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Foreign Key relationships
        builder.HasOne(e => e.Conversation)
            .WithMany(c => c.CustomerVendorMessages)
            .HasForeignKey(e => e.ConversationId)
            .HasConstraintName("fk_customer_vendor_messages_conversation_id")
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(e => new { e.ConversationId, e.CreatedAt })
            .HasDatabaseName("idx_conversation_created");

        builder.HasIndex(e => new { e.ConversationId, e.IsRead })
            .HasDatabaseName("idx_conversation_unread");
    }
}

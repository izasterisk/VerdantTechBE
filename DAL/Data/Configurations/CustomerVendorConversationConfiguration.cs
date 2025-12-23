using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAL.Data.Models;

namespace DAL.Data.Configurations;

public class CustomerVendorConversationConfiguration : IEntityTypeConfiguration<CustomerVendorConversation>
{
    public void Configure(EntityTypeBuilder<CustomerVendorConversation> builder)
    {
        // Primary Key
        builder.HasKey(e => e.Id);

        // Table configuration
        builder.ToTable("customer_vendor_conversations");

        // Property configurations
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(e => e.CustomerId)
            .HasColumnName("customer_id")
            .HasColumnType("bigint unsigned")
            .IsRequired();

        builder.Property(e => e.VendorId)
            .HasColumnName("vendor_id")
            .HasColumnType("bigint unsigned")
            .IsRequired();

        builder.Property(e => e.StartedAt)
            .HasColumnName("started_at")
            .HasColumnType("timestamp")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.LastMessageAt)
            .HasColumnName("last_message_at")
            .HasColumnType("timestamp");

        // Foreign Key relationships
        builder.HasOne(e => e.Customer)
            .WithMany(u => u.CustomerVendorConversationsAsCustomer)
            .HasForeignKey(e => e.CustomerId)
            .HasConstraintName("fk_customer_vendor_conversations_customer_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Vendor)
            .WithMany(u => u.CustomerVendorConversationsAsVendor)
            .HasForeignKey(e => e.VendorId)
            .HasConstraintName("fk_customer_vendor_conversations_vendor_id")
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(e => new { e.CustomerId, e.VendorId })
            .HasDatabaseName("idx_customer_vendor")
            .IsUnique();

        builder.HasIndex(e => new { e.CustomerId, e.LastMessageAt })
            .HasDatabaseName("idx_customer_last");

        builder.HasIndex(e => new { e.VendorId, e.LastMessageAt })
            .HasDatabaseName("idx_vendor_last");
    }
}

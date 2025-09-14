using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using DAL.Data.Models;
using DAL.Data;

namespace DAL.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        
        // Primary Key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("bigint unsigned")
            .ValueGeneratedOnAdd();
        
        // Foreign Key
        builder.Property(e => e.CustomerId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("customer_id");
        
        // Enum conversion for status
        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasColumnType("enum('pending','confirmed','processing','shipped','delivered','cancelled','refunded')")
            .HasDefaultValue(OrderStatus.Pending);
        
        // Decimal fields with precision
        builder.Property(e => e.Subtotal)
            .HasPrecision(12, 2)
            .IsRequired();
            
        builder.Property(e => e.TaxAmount)
            .HasPrecision(12, 2)
            .HasDefaultValue(0.00m)
            .HasColumnName("tax_amount");
            
        builder.Property(e => e.ShippingFee)
            .HasPrecision(12, 2)
            .HasDefaultValue(0.00m)
            .HasColumnName("shipping_fee");
            
        builder.Property(e => e.DiscountAmount)
            .HasPrecision(12, 2)
            .HasDefaultValue(0.00m)
            .HasColumnName("discount_amount");
            
        builder.Property(e => e.TotalAmount)
            .HasPrecision(12, 2)
            .IsRequired()
            .HasColumnName("total_amount");
        
        // JSON field for shipping address using JsonHelpers
        builder.Property(e => e.ShippingAddress)
            .HasConversion(JsonHelpers.DictionaryStringObjectConverter())
            .HasColumnType("json")
            .IsRequired()
            .HasColumnName("shipping_address")
            .Metadata.SetValueComparer(JsonHelpers.DictionaryStringObjectComparer());
            
        // Optional string fields
        builder.Property(e => e.ShippingMethod)
            .HasMaxLength(100)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("shipping_method");
            
        builder.Property(e => e.TrackingNumber)
            .HasMaxLength(100)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("tracking_number");
            
        builder.Property(e => e.Notes)
            .HasColumnType("text")
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci");
            
        builder.Property(e => e.CancelledReason)
            .HasColumnType("text")
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("cancelled_reason");
        
        // DateTime fields
        builder.Property(e => e.CancelledAt)
            .HasColumnType("timestamp")
            .HasColumnName("cancelled_at");
            
        builder.Property(e => e.ConfirmedAt)
            .HasColumnType("timestamp")
            .HasColumnName("confirmed_at");
            
        builder.Property(e => e.ShippedAt)
            .HasColumnType("timestamp")
            .HasColumnName("shipped_at");
            
        builder.Property(e => e.DeliveredAt)
            .HasColumnType("timestamp")
            .HasColumnName("delivered_at");
            
        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at");
        
        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
            .HasColumnName("updated_at");
        
        // Foreign Key Relationships
        // Foreign Key Relationships
        builder.HasOne(d => d.Customer)
            .WithMany(p => p.CustomerOrders)
            .HasForeignKey(d => d.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Indexes
        builder.HasIndex(e => e.CustomerId)
            .HasDatabaseName("idx_customer");
            
        builder.HasIndex(e => e.Status)
            .HasDatabaseName("idx_status");
            
        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("idx_created_at");
    }
}

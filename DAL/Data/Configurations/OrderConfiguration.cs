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
        
        // Foreign Keys
        builder.Property(e => e.CustomerId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("customer_id");
            
        builder.Property(e => e.VendorId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("vendor_id");
        
        // Required unique field
        builder.Property(e => e.OrderNumber)
            .HasMaxLength(50)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("order_number");
        
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
        
        // Currency field
        builder.Property(e => e.CurrencyCode)
            .HasMaxLength(3)
            .HasDefaultValue("VND")
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("currency_code");
        
        // JSON fields for addresses
        builder.Property(e => e.ShippingAddress)
            .HasConversion(
                v => v == null || v.Count == 0 ? "{}" : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v) || v == "{}" ? new Dictionary<string, object>() : JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("json")
            .IsRequired()
            .HasColumnName("shipping_address");
            
        builder.Property(e => e.BillingAddress)
            .HasConversion(
                v => v == null || v.Count == 0 ? "{}" : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v) || v == "{}" ? new Dictionary<string, object>() : JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("json")
            .HasColumnName("billing_address");
        
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
        builder.HasOne(d => d.Customer)
            .WithMany(p => p.CustomerOrders)
            .HasForeignKey(d => d.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(d => d.Vendor)
            .WithMany(p => p.VendorOrders)
            .HasForeignKey(d => d.VendorId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Unique constraint
        builder.HasIndex(e => e.OrderNumber)
            .IsUnique()
            .HasDatabaseName("idx_order_number");
        
        // Indexes
        builder.HasIndex(e => e.CustomerId)
            .HasDatabaseName("idx_customer");
            
        builder.HasIndex(e => e.VendorId)
            .HasDatabaseName("idx_vendor");
            
        builder.HasIndex(e => e.Status)
            .HasDatabaseName("idx_status");
            
        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("idx_created_at");
    }
}

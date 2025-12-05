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
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
        
        // Foreign Keys
        builder.Property(e => e.CustomerId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("customer_id");
            
        builder.Property(e => e.AddressId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("address_id");
            
        // Enum conversion for order payment method
        builder.Property(e => e.OrderPaymentMethod)
            .HasConversion<string>()
            .HasColumnType("enum('Banking','COD','Rent')")
            .HasColumnName("order_payment_method")
            .IsRequired();
        
        // Enum conversion for status
        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasColumnType("enum('pending','processing','paid','shipped','delivered','finished','cancelled','refunded')")
            .HasColumnName("status")
            .HasDefaultValue(OrderStatus.Pending);
        
        // Decimal fields with precision
        builder.Property(e => e.Subtotal)
            .HasPrecision(12, 2)
            .HasColumnName("subtotal")
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
            
        builder.Property(e => e.CourierId)
            .HasColumnType("int")
            .HasColumnName("courier_id");
            
        builder.Property(e => e.Width)
            .HasColumnType("int")
            .HasColumnName("width");
            
        builder.Property(e => e.Height)
            .HasColumnType("int")
            .HasColumnName("height");
            
        builder.Property(e => e.Length)
            .HasColumnType("int")
            .HasColumnName("length");
            
        builder.Property(e => e.Weight)
            .HasColumnType("int")
            .HasColumnName("weight");
            
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
            .WithMany(p => p.Orders)
            .HasForeignKey(d => d.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(d => d.Address)
            .WithMany(p => p.Orders)
            .HasForeignKey(d => d.AddressId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Indexes
        builder.HasIndex(e => e.CustomerId)
            .HasDatabaseName("idx_customer");
            
        builder.HasIndex(e => e.AddressId)
            .HasDatabaseName("idx_address");
            
        builder.HasIndex(e => e.Status)
            .HasDatabaseName("idx_status");
            
        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("idx_created");
            
        // Composite index for vendor revenue queries
        builder.HasIndex(e => new { e.Status, e.CreatedAt })
            .HasDatabaseName("idx_status_created");
    }
}

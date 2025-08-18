using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using DAL.Data.Models;

namespace DAL.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");
        
        // Primary Key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("bigint unsigned")
            .ValueGeneratedOnAdd();
        
        // Foreign Keys
        builder.Property(e => e.OrderId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("order_id");
            
        builder.Property(e => e.ProductId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("product_id");
        
        // Snapshot fields - required
        builder.Property(e => e.ProductName)
            .HasMaxLength(255)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("product_name");
            
        builder.Property(e => e.ProductSku)
            .HasMaxLength(100)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("product_sku");
        
        // Quantity and pricing
        builder.Property(e => e.Quantity)
            .IsRequired();
            
        builder.Property(e => e.UnitPrice)
            .HasPrecision(12, 2)
            .IsRequired()
            .HasColumnName("unit_price");
            
        builder.Property(e => e.DiscountAmount)
            .HasPrecision(12, 2)
            .HasDefaultValue(0.00m)
            .HasColumnName("discount_amount");
            
        builder.Property(e => e.Subtotal)
            .HasPrecision(12, 2)
            .IsRequired();
        
        // JSON field for specifications snapshot
        builder.Property(e => e.Specifications)
            .HasConversion(
                v => v == null || v.Count == 0 ? "{}" : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v) || v == "{}" ? new Dictionary<string, object>() : JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("json")
            .HasDefaultValueSql("'{}'");
        
        // DateTime field
        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at");
        
        // Foreign Key Relationships
        builder.HasOne(d => d.Order)
            .WithMany(p => p.OrderItems)
            .HasForeignKey(d => d.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(d => d.Product)
            .WithMany(p => p.OrderItems)
            .HasForeignKey(d => d.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Indexes
        builder.HasIndex(e => e.OrderId)
            .HasDatabaseName("idx_order");
            
        builder.HasIndex(e => e.ProductId)
            .HasDatabaseName("idx_product");
    }
}

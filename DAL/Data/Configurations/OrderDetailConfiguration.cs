using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAL.Data.Models;

namespace DAL.Data.Configurations;

public class OrderDetailConfiguration : IEntityTypeConfiguration<OrderDetail>
{
    public void Configure(EntityTypeBuilder<OrderDetail> builder)
    {
        builder.ToTable("order_details");
        
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
        
        // IsWalletCredited field
        builder.Property(e => e.IsWalletCredited)
            .HasDefaultValue(false)
            .HasColumnName("is_wallet_credited");
        
        // DateTime field
        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp")
            .ValueGeneratedOnAddOrUpdate()
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("updated_at");
        
        // Foreign Key Relationships
        builder.HasOne(d => d.Order)
            .WithMany(p => p.OrderDetails)
            .HasForeignKey(d => d.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(d => d.Product)
            .WithMany(p => p.OrderDetails)
            .HasForeignKey(d => d.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Indexes
        builder.HasIndex(e => e.OrderId)
            .HasDatabaseName("idx_order");
            
        builder.HasIndex(e => e.ProductId)
            .HasDatabaseName("idx_product");
        
        // Composite index for wallet credit queries
        builder.HasIndex(e => new { e.IsWalletCredited, e.UpdatedAt })
            .HasDatabaseName("idx_wallet_credited_updated");
    }
}

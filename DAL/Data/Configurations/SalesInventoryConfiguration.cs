using DAL.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class SalesInventoryConfiguration : IEntityTypeConfiguration<SalesInventory>
{
    public void Configure(EntityTypeBuilder<SalesInventory> builder)
    {
        builder.ToTable("sales_inventory");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasColumnType("bigint unsigned")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.ProductId)
            .HasColumnName("product_id")
            .HasColumnType("bigint unsigned")
            .IsRequired();

        builder.Property(e => e.OrderId)
            .HasColumnName("order_id")
            .HasColumnType("bigint unsigned");

        builder.Property(e => e.Quantity)
            .HasColumnName("quantity")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.UnitSalePrice)
            .HasColumnName("unit_sale_price")
            .HasColumnType("decimal(12,2)")
            .IsRequired();

        builder.Property(e => e.TotalRevenue)
            .HasColumnName("total_revenue")
            .HasColumnType("decimal(12,2)")
            .IsRequired();

        builder.Property(e => e.CommissionAmount)
            .HasColumnName("commission_amount")
            .HasColumnType("decimal(12,2)")
            .HasDefaultValue(0.00m);

        builder.Property(e => e.BalanceAfter)
            .HasColumnName("balance_after")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.MovementType)
            .HasConversion<string>()
            .HasColumnName("movement_type")
            .HasColumnType("enum('sale','return','damage','loss','adjustment')")
            .HasDefaultValue(MovementType.Sale);

        builder.Property(e => e.Notes)
            .HasColumnName("notes")
            .HasColumnType("text");

        builder.Property(e => e.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("bigint unsigned")
            .IsRequired();

        builder.Property(e => e.SoldAt)
            .HasColumnName("sold_at")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Foreign keys
        builder.HasOne(e => e.Product)
            .WithMany(p => p.SalesInventories)
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Order)
            .WithMany()
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.CreatedByNavigation)
            .WithMany()
            .HasForeignKey(e => e.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(e => e.ProductId).HasDatabaseName("idx_product");
        builder.HasIndex(e => e.OrderId).HasDatabaseName("idx_order");
        builder.HasIndex(e => e.MovementType).HasDatabaseName("idx_movement_type");
        builder.HasIndex(e => e.SoldAt).HasDatabaseName("idx_sold_date");
        builder.HasIndex(e => e.CreatedBy).HasDatabaseName("idx_created_by");
    }
}

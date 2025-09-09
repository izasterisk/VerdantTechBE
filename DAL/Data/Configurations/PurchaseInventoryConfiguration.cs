using DAL.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class PurchaseInventoryConfiguration : IEntityTypeConfiguration<PurchaseInventory>
{
    public void Configure(EntityTypeBuilder<PurchaseInventory> builder)
    {
        builder.ToTable("purchase_inventory");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasColumnType("bigint unsigned")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.ProductId)
            .HasColumnName("product_id")
            .HasColumnType("bigint unsigned")
            .IsRequired();

        builder.Property(e => e.VendorProfileId)
            .HasColumnName("vendor_profile_id")
            .HasColumnType("bigint unsigned");

        builder.Property(e => e.Quantity)
            .HasColumnName("quantity")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.UnitCostPrice)
            .HasColumnName("unit_cost_price")
            .HasColumnType("decimal(12,2)")
            .IsRequired();

        builder.Property(e => e.TotalCost)
            .HasColumnName("total_cost")
            .HasColumnType("decimal(12,2)")
            .IsRequired();

        builder.Property(e => e.CommissionRate)
            .HasColumnName("commission_rate")
            .HasColumnType("decimal(5,2)")
            .HasDefaultValue(0.00m)
            .HasComment("Commission rate for this purchase");

        builder.Property(e => e.BatchNumber)
            .HasColumnName("batch_number")
            .HasColumnType("varchar(100)")
            .HasMaxLength(100);

        builder.Property(e => e.SupplierInvoice)
            .HasColumnName("supplier_invoice")
            .HasColumnType("varchar(255)")
            .HasMaxLength(255);

        builder.Property(e => e.Notes)
            .HasColumnName("notes")
            .HasColumnType("text");

        builder.Property(e => e.BalanceAfter)
            .HasColumnName("balance_after")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("bigint unsigned")
            .IsRequired();

        builder.Property(e => e.PurchasedAt)
            .HasColumnName("purchased_at")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

        // Foreign keys
        builder.HasOne(e => e.Product)
            .WithMany(p => p.PurchaseInventories)
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.VendorProfile)
            .WithMany(v => v.PurchaseInventories)
            .HasForeignKey(e => e.VendorProfileId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.CreatedByNavigation)
            .WithMany()
            .HasForeignKey(e => e.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(e => e.ProductId).HasDatabaseName("idx_product");
        builder.HasIndex(e => e.VendorProfileId).HasDatabaseName("idx_vendor");
        builder.HasIndex(e => e.PurchasedAt).HasDatabaseName("idx_purchased_date");
        builder.HasIndex(e => e.CreatedBy).HasDatabaseName("idx_created_by");
        builder.HasIndex(e => e.BatchNumber).HasDatabaseName("idx_batch");
    }
}

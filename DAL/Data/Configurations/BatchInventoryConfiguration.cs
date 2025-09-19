using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAL.Data.Models;

namespace DAL.Data.Configurations;

public class BatchInventoryConfiguration : IEntityTypeConfiguration<BatchInventory>
{
    public void Configure(EntityTypeBuilder<BatchInventory> builder)
    {
        builder.ToTable("batch_inventory");

        // Primary Key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("bigint unsigned")
            .ValueGeneratedOnAdd();

        // Foreign Keys
        builder.Property(e => e.ProductId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("product_id");

        builder.Property(e => e.VendorId)
            .HasColumnType("bigint unsigned")
            .HasColumnName("vendor_id");

        builder.Property(e => e.QualityCheckedBy)
            .HasColumnType("bigint unsigned")
            .HasColumnName("quality_checked_by");

        // Required fields
        builder.Property(e => e.Sku)
            .HasMaxLength(100)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci");

        builder.Property(e => e.Quantity)
            .IsRequired();

        builder.Property(e => e.UnitCostPrice)
            .HasColumnType("decimal(12,2)")
            .IsRequired()
            .HasColumnName("unit_cost_price");

        // Optional fields
        builder.Property(e => e.BatchNumber)
            .HasMaxLength(100)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("batch_number");

        builder.Property(e => e.LotNumber)
            .HasMaxLength(100)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("lot_number");

        builder.Property(e => e.Notes)
            .HasMaxLength(500)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci");

        // Date fields
        builder.Property(e => e.ExpiryDate)
            .HasColumnType("date")
            .HasColumnName("expiry_date");

        builder.Property(e => e.ManufacturingDate)
            .HasColumnType("date")
            .HasColumnName("manufacturing_date");

        // Enum conversion for quality check status
        builder.Property(e => e.QualityCheckStatus)
            .HasConversion(
                v => v.ToString().ToLowerInvariant().Replace("notrequired", "not_required"),
                v => Enum.Parse<QualityCheckStatus>(v.Replace("not_required", "NotRequired"), true))
            .HasColumnType("enum('pending','passed','failed','not_required')")
            .HasDefaultValue(QualityCheckStatus.NotRequired)
            .HasColumnName("quality_check_status");

        // DateTime fields
        builder.Property(e => e.QualityCheckedAt)
            .HasColumnType("timestamp")
            .HasColumnName("quality_checked_at");

        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at");

        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
            .HasColumnName("updated_at");

        // Foreign Key Relationships
        builder.HasOne(d => d.Product)
            .WithMany(p => p.BatchInventories)
            .HasForeignKey(d => d.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Vendor)
            .WithMany(p => p.BatchInventoriesAsVendor)
            .HasForeignKey(d => d.VendorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.QualityCheckedByNavigation)
            .WithMany(p => p.BatchInventoriesQualityChecked)
            .HasForeignKey(d => d.QualityCheckedBy)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(e => e.ProductId)
            .HasDatabaseName("idx_product");

        builder.HasIndex(e => e.Sku)
            .HasDatabaseName("idx_sku");

        builder.HasIndex(e => e.VendorId)
            .HasDatabaseName("idx_vendor");

        builder.HasIndex(e => e.ExpiryDate)
            .HasDatabaseName("idx_expiry_date");

        builder.HasIndex(e => e.QualityCheckStatus)
            .HasDatabaseName("idx_quality_status");
    }
}

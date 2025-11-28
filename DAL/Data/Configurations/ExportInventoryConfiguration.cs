using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAL.Data.Models;

namespace DAL.Data.Configurations;

public class ExportInventoryConfiguration : IEntityTypeConfiguration<ExportInventory>
{
    public void Configure(EntityTypeBuilder<ExportInventory> builder)
    {
        builder.ToTable("export_inventory");

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

        builder.Property(e => e.ProductSerialId)
            .HasColumnType("bigint unsigned")
            .HasColumnName("product_serial_id");

        builder.Property(e => e.LotNumber)
            .IsRequired()
            .HasMaxLength(100)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("lot_number");

        builder.Property(e => e.OrderDetailId)
            .HasColumnType("bigint unsigned")
            .HasColumnName("order_detail_id");

        builder.Property(e => e.Quantity)
            .IsRequired()
            .HasDefaultValue(1)
            .HasColumnName("quantity");

        builder.Property(e => e.RefundQuantity)
            .IsRequired()
            .HasDefaultValue(0)
            .HasColumnName("refund_quantity");

        builder.Property(e => e.CreatedBy)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("created_by");

        // Enum conversion for movement type
        builder.Property(e => e.MovementType)
            .HasConversion(
                v => v.ToString()
                    .ToLowerInvariant()
                    .Replace("returntovendor", "return to vendor"),
                v => Enum.Parse<MovementType>(v
                    .Replace("return to vendor", "ReturnToVendor"), true))
            .HasColumnType("enum('sale','return to vendor','damage','loss','adjustment')")
            .HasDefaultValue(MovementType.Sale)
            .HasColumnName("movement_type");

        // Optional fields
        builder.Property(e => e.Notes)
            .HasMaxLength(500)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci");

        // DateTime fields
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
            .WithMany(p => p.ExportInventories)
            .HasForeignKey(d => d.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.ProductSerial)
            .WithOne(p => p.ExportInventory)
            .HasForeignKey<ExportInventory>(d => d.ProductSerialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.OrderDetail)
            .WithMany(p => p.ExportInventories)
            .HasForeignKey(d => d.OrderDetailId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(d => d.CreatedByNavigation)
            .WithMany(p => p.ExportInventories)
            .HasForeignKey(d => d.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(e => e.ProductId)
            .HasDatabaseName("idx_product");

        builder.HasIndex(e => e.OrderDetailId)
            .HasDatabaseName("idx_order_detail");

        builder.HasIndex(e => e.ProductSerialId)
            .HasDatabaseName("idx_serial");

        builder.HasIndex(e => e.LotNumber)
            .HasDatabaseName("idx_lot");

        // Composite index for ProductId and LotNumber (used in inventory queries)
        builder.HasIndex(e => new { e.ProductId, e.LotNumber })
            .HasDatabaseName("idx_product_lot");
    }
}

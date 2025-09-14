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

        builder.Property(e => e.Sku)
            .HasColumnName("sku")
            .HasColumnType("varchar(100)")
            .HasMaxLength(100)
            .IsRequired()
            .HasComment("Mã quản lý kho - mã nhận dạng duy nhất cho lô hàng này");

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
            .HasMaxLength(100)
            .HasComment("Số lô hoặc số lô hàng để theo dõi");

        builder.Property(e => e.LotNumber)
            .HasColumnName("lot_number")
            .HasColumnType("varchar(100)")
            .HasMaxLength(100)
            .HasComment("Số lô sản xuất");

        builder.Property(e => e.SerialNumbers)
            .HasColumnName("serial_numbers")
            .HasColumnType("text")
            .HasComment("Số serial của từng sản phẩm, phân cách bằng dấu phẩy");

        builder.Property(e => e.SupplierInvoice)
            .HasColumnName("supplier_invoice")
            .HasColumnType("varchar(255)")
            .HasMaxLength(255)
            .HasComment("Tham chiếu hóa đơn nhà cung cấp");

        builder.Property(e => e.PurchaseOrderNumber)
            .HasColumnName("purchase_order_number")
            .HasColumnType("varchar(100)")
            .HasMaxLength(100)
            .HasComment("Số tham chiếu đơn đặt hàng");

        builder.Property(e => e.WarehouseLocation)
            .HasColumnName("warehouse_location")
            .HasColumnType("varchar(100)")
            .HasMaxLength(100)
            .HasComment("Vị trí lưu trữ trong kho (kệ, khu vực, v.v.)");

        builder.Property(e => e.ExpiryDate)
            .HasColumnName("expiry_date")
            .HasColumnType("date")
            .HasComment("Ngày hết hạn sản phẩm nếu có");

        builder.Property(e => e.ManufacturingDate)
            .HasColumnName("manufacturing_date")
            .HasColumnType("date")
            .HasComment("Ngày sản xuất nếu có");

        builder.Property(e => e.QualityCheckStatus)
            .HasColumnName("quality_check_status")
            .HasColumnType("enum('pending','passed','failed','not_required')")
            .HasDefaultValue(Data.QualityCheckStatus.NotRequired)
            .HasConversion<string>()
            .HasComment("Trạng thái kiểm tra chất lượng");

        builder.Property(e => e.QualityCheckNotes)
            .HasColumnName("quality_check_notes")
            .HasColumnType("varchar(500)")
            .HasMaxLength(500)
            .HasComment("Ghi chú kiểm tra chất lượng");

        builder.Property(e => e.QualityCheckedBy)
            .HasColumnName("quality_checked_by")
            .HasColumnType("bigint unsigned")
            .HasComment("Người thực hiện kiểm tra chất lượng");

        builder.Property(e => e.QualityCheckedAt)
            .HasColumnName("quality_checked_at")
            .HasColumnType("timestamp")
            .HasComment("Khi nào kiểm tra chất lượng được thực hiện");

        builder.Property(e => e.ConditionOnArrival)
            .HasColumnName("condition_on_arrival")
            .HasColumnType("enum('new','good','fair','damaged')")
            .HasDefaultValue(Data.ConditionOnArrival.New)
            .HasConversion<string>()
            .HasComment("Tình trạng hàng hóa khi nhận");

        builder.Property(e => e.DamageNotes)
            .HasColumnName("damage_notes")
            .HasColumnType("varchar(500)")
            .HasMaxLength(500)
            .HasComment("Ghi chú về bất kỳ hư hỏng hoặc vấn đề nào");

        builder.Property(e => e.Notes)
            .HasColumnName("notes")
            .HasColumnType("varchar(500)")
            .HasMaxLength(500);

        builder.Property(e => e.BalanceAfter)
            .HasColumnName("balance_after")
            .HasColumnType("int")
            .IsRequired()
            .HasComment("Số dư tồn kho sau lần mua này");

        builder.Property(e => e.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasComment("Người ghi nhận lần mua này");

        builder.Property(e => e.PurchasedAt)
            .HasColumnName("purchased_at")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.ReceivedAt)
            .HasColumnName("received_at")
            .HasColumnType("timestamp")
            .HasComment("Khi hàng hóa được nhận về thực tế");

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

        builder.HasOne(e => e.QualityCheckedByNavigation)
            .WithMany()
            .HasForeignKey(e => e.QualityCheckedBy)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(e => e.ProductId).HasDatabaseName("idx_product");
        builder.HasIndex(e => e.Sku).HasDatabaseName("idx_sku");
        builder.HasIndex(e => e.VendorProfileId).HasDatabaseName("idx_vendor");
        builder.HasIndex(e => e.PurchasedAt).HasDatabaseName("idx_purchased_date");
        builder.HasIndex(e => e.ReceivedAt).HasDatabaseName("idx_received_date");
        builder.HasIndex(e => e.ExpiryDate).HasDatabaseName("idx_expiry_date");
        builder.HasIndex(e => e.QualityCheckStatus).HasDatabaseName("idx_quality_status");
        builder.HasIndex(e => e.ConditionOnArrival).HasDatabaseName("idx_condition");
        builder.HasIndex(e => e.CreatedBy).HasDatabaseName("idx_created_by");
        builder.HasIndex(e => e.BatchNumber).HasDatabaseName("idx_batch");
        builder.HasIndex(e => e.LotNumber).HasDatabaseName("idx_lot");
        builder.HasIndex(e => e.WarehouseLocation).HasDatabaseName("idx_warehouse_location");
    }
}

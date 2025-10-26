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

        builder.Property(e => e.OrderId)
            .HasColumnType("bigint unsigned")
            .HasColumnName("order_id");

        builder.Property(e => e.CreatedBy)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("created_by");

        // Required fields
        builder.Property(e => e.Quantity)
            .IsRequired();

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

        builder.HasOne(d => d.Order)
            .WithMany(p => p.ExportInventories)
            .HasForeignKey(d => d.OrderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(d => d.CreatedByNavigation)
            .WithMany(p => p.ExportInventories)
            .HasForeignKey(d => d.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(e => e.ProductId)
            .HasDatabaseName("idx_product");

        builder.HasIndex(e => e.OrderId)
            .HasDatabaseName("idx_order");

        builder.HasIndex(e => e.MovementType)
            .HasDatabaseName("idx_movement_type");
    }
}

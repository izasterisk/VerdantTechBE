using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAL.Data.Models;

namespace DAL.Data.Configurations;

public class ProductSerialConfiguration : IEntityTypeConfiguration<ProductSerial>
{
    public void Configure(EntityTypeBuilder<ProductSerial> builder)
    {
        builder.ToTable("product_serials");

        // Primary Key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("bigint unsigned")
            .ValueGeneratedOnAdd();

        // Foreign Keys
        builder.Property(e => e.BatchInventoryId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("batch_inventory_id");

        builder.Property(e => e.ProductId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("product_id");

        // Required fields
        builder.Property(e => e.SerialNumber)
            .HasMaxLength(255)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("serial_number");

        // Unique constraint on serial_number
        builder.HasIndex(e => e.SerialNumber)
            .IsUnique()
            .HasDatabaseName("serial_number");

        // Enum conversion for status
        builder.Property(e => e.Status)
            .HasConversion(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<ProductSerialStatus>(v, true))
            .HasColumnType("enum('stock','sold','refund')")
            .HasDefaultValue(ProductSerialStatus.Stock)
            .HasColumnName("status");

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
        builder.HasOne(d => d.BatchInventory)
            .WithMany(p => p.ProductSerials)
            .HasForeignKey(d => d.BatchInventoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Product)
            .WithMany(p => p.ProductSerials)
            .HasForeignKey(d => d.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(e => e.BatchInventoryId)
            .HasDatabaseName("idx_batch");

        builder.HasIndex(e => e.ProductId)
            .HasDatabaseName("idx_product");
    }
}

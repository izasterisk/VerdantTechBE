using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using DAL.Data.Models;

namespace DAL.Data.Configurations;

public class ProductUpdateRequestConfiguration : IEntityTypeConfiguration<ProductUpdateRequest>
{
    public void Configure(EntityTypeBuilder<ProductUpdateRequest> builder)
    {
        builder.ToTable("product_update_requests");

        // PK
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("bigint unsigned")
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // Required FKs
        builder.Property(e => e.ProductId)
            .HasColumnType("bigint unsigned")
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(e => e.VendorId)
            .HasColumnType("bigint unsigned")
            .HasColumnName("vendor_id")
            .IsRequired();

        builder.Property(e => e.CategoryId)
            .HasColumnType("bigint unsigned")
            .HasColumnName("category_id")
            .IsRequired();

        // Strings
        builder.Property(e => e.ProductCode)
            .HasMaxLength(100)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("product_code")
            .IsRequired();

        builder.Property(e => e.ProductName)
            .HasMaxLength(255)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("product_name")
            .IsRequired();

        builder.Property(e => e.Slug)
            .HasMaxLength(255)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("slug")
            .IsRequired();

        builder.Property(e => e.Description)
            .HasColumnType("text")
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("description");

        // Pricing
        builder.Property(e => e.UnitPrice)
            .HasPrecision(12, 2)
            .HasColumnType("decimal(12,2)")
            .HasColumnName("unit_price")
            .IsRequired();

        builder.Property(e => e.CommissionRate)
            .HasPrecision(5, 2)
            .HasColumnType("decimal(5,2)")
            .HasColumnName("commission_rate")
            .HasDefaultValue(0.00m);

        builder.Property(e => e.DiscountPercentage)
            .HasPrecision(5, 2)
            .HasColumnType("decimal(5,2)")
            .HasColumnName("discount_percentage")
            .HasDefaultValue(0.00m);

        builder.Property(e => e.EnergyEfficiencyRating)
            .HasColumnType("int")
            .HasColumnName("energy_efficiency_rating");

        builder.Property(e => e.ManualUrls)
            .HasMaxLength(1000)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("manual_urls");

        builder.Property(e => e.PublicUrl)
            .HasMaxLength(500)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("public_url")
            .IsRequired(false);

        builder.Property(e => e.WarrantyMonths)
            .HasColumnType("int")
            .HasColumnName("warranty_months")
            .HasDefaultValue(12);

        builder.Property(e => e.WeightKg)
            .IsRequired()
            .HasPrecision(10, 3)
            .HasColumnType("decimal(10,3)")
            .HasColumnName("weight_kg");

        // JSON fields - Using JsonHelpers for converter and comparer
        builder.Property(e => e.Specifications)
            .ConfigureAsJson("specifications");
            
        builder.Property(e => e.DimensionsCm)
            .ConfigureAsJson("dimensions_cm");

        // ===== Enum Status: lowercase in DB <-> enum in code =====
        var statusConverter = new ValueConverter<ProductRegistrationStatus, string>(
            v => v.ToString().ToLowerInvariant(),                      // enum -> "pending|approved|rejected"
            v => Enum.Parse<ProductRegistrationStatus>(v, true)        // "pending" -> ProductRegistrationStatus.Pending
        );

        builder.Property(e => e.Status)
            .HasConversion(statusConverter)
            .HasColumnName("status")
            .HasColumnType("enum('pending','approved','rejected')")
            .IsRequired();

        builder.Property(e => e.RejectionReason)
            .HasMaxLength(500)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("rejection_reason");

        builder.Property(e => e.ProcessedBy)
            .HasColumnType("bigint unsigned")
            .HasColumnName("processed_by");

        // DateTimes
        builder.Property(e => e.ProcessedAt)
            .HasColumnType("timestamp")
            .HasColumnName("processed_at");

        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at");

        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp")
            .ValueGeneratedOnAddOrUpdate()
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("updated_at");

        // Indexes
        builder.HasIndex(e => e.ProductId).HasDatabaseName("idx_product");
        builder.HasIndex(e => e.VendorId).HasDatabaseName("idx_vendor");
        builder.HasIndex(e => e.Status).HasDatabaseName("idx_status");

        // FKs
        builder.HasOne(e => e.Product)
            .WithMany()
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_product_update_requests__product_id__products__id");

        builder.HasOne(e => e.Vendor)
            .WithMany()
            .HasForeignKey(e => e.VendorId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_product_update_requests__vendor_id__users__id");

        builder.HasOne(e => e.Category)
            .WithMany()
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_product_update_requests__category_id__product_categories__id");

        builder.HasOne(e => e.ProcessedByUser)
            .WithMany()
            .HasForeignKey(e => e.ProcessedBy)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_product_update_requests__processed_by__users__id");
    }
}

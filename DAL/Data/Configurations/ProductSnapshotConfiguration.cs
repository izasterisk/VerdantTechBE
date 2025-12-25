using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAL.Data.Models;

namespace DAL.Data.Configurations;

public class ProductSnapshotConfiguration : IEntityTypeConfiguration<ProductSnapshot>
{
    public void Configure(EntityTypeBuilder<ProductSnapshot> builder)
    {
        builder.ToTable("product_snapshot");

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

        builder.Property(e => e.CategoryId)
            .HasColumnType("bigint unsigned")
            .HasColumnName("category_id")
            .IsRequired();

        builder.Property(e => e.VendorId)
            .HasColumnType("bigint unsigned")
            .HasColumnName("vendor_id")
            .IsRequired();

        builder.Property(e => e.RegistrationId)
            .HasColumnType("bigint unsigned")
            .HasColumnName("registration_id");

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

        builder.Property(e => e.ManualUrls)
            .HasMaxLength(1000)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("manual_urls");

        builder.Property(e => e.PublicUrl)
            .HasMaxLength(500)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("public_url");

        // Decimals
        builder.Property(e => e.UnitPrice)
            .HasColumnType("decimal(12,2)")
            .HasColumnName("unit_price")
            .IsRequired();

        builder.Property(e => e.CommissionRate)
            .HasColumnType("decimal(5,2)")
            .HasColumnName("commission_rate")
            .HasDefaultValue(0.00m);

        builder.Property(e => e.DiscountPercentage)
            .HasColumnType("decimal(5,2)")
            .HasColumnName("discount_percentage")
            .HasDefaultValue(0.00m);

        builder.Property(e => e.WeightKg)
            .HasColumnType("decimal(10,3)")
            .HasColumnName("weight_kg")
            .IsRequired();

        // Integers
        builder.Property(e => e.EnergyEfficiencyRating)
            .HasColumnType("int")
            .HasColumnName("energy_efficiency_rating");

        builder.Property(e => e.WarrantyMonths)
            .HasColumnType("int")
            .HasColumnName("warranty_months")
            .HasDefaultValue(12);

        // JSON columns - Using JsonHelpers for converter and comparer
        builder.Property(e => e.Specifications)
            .ConfigureAsJson("specifications");

        builder.Property(e => e.DimensionsCm)
            .ConfigureAsJson("dimensions_cm");

        // Enum
        builder.Property(e => e.SnapshotType)
            .HasConversion(
                v => v.ToString().ToLowerInvariant()
                    .Replace("subscriptionbanned", "subscription_banned"),
                v => Enum.Parse<ProductSnapshotType>(v
                    .Replace("subscription_banned", "SubscriptionBanned"), true))
            .HasColumnType("enum('proposed','history','subscription_banned')")
            .HasColumnName("snapshot_type")
            .IsRequired();

        // Timestamps
        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp")
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp")
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAddOrUpdate();

        // Foreign Keys
        builder.HasOne(e => e.Product)
            .WithMany()
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("product_snapshot_ibfk_1");

        builder.HasOne(e => e.Category)
            .WithMany()
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("product_snapshot_ibfk_2");

        builder.HasOne(e => e.Vendor)
            .WithMany()
            .HasForeignKey(e => e.VendorId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("product_snapshot_ibfk_3");

        builder.HasOne(e => e.Registration)
            .WithMany()
            .HasForeignKey(e => e.RegistrationId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("product_snapshot_ibfk_4");

        // 1:1 Relationship with ProductUpdateRequest
        builder.HasOne(e => e.ProductUpdateRequest)
            .WithOne(p => p.ProductSnapshot)
            .HasForeignKey<ProductUpdateRequest>(p => p.ProductSnapshotId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        // Composite index cho history pagination
        builder.HasIndex(e => new { e.ProductId, e.SnapshotType, e.CreatedAt })
            .HasDatabaseName("idx_product_snapshot_created");
        
        // Composite index cho queries filter theo vendor và snapshot type
        builder.HasIndex(e => new { e.VendorId, e.SnapshotType })
            .HasDatabaseName("idx_vendor_snapshot");
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAL.Data.Models;

namespace DAL.Data.Configurations;

public class ProductRegistrationConfiguration : IEntityTypeConfiguration<ProductRegistration>
{
    public void Configure(EntityTypeBuilder<ProductRegistration> builder)
    {
        builder.ToTable("product_registrations");
        
        // Primary Key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("bigint unsigned")
            .ValueGeneratedOnAdd();
        
        // Required fields
        builder.Property(e => e.VendorId)
            .HasColumnType("bigint unsigned")
            .HasColumnName("vendor_id")
            .IsRequired();
            
        builder.Property(e => e.CategoryId)
            .HasColumnType("bigint unsigned")
            .HasColumnName("category_id")
            .IsRequired();
            
        builder.Property(e => e.ProposedProductCode)
            .HasMaxLength(100)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("proposed_product_code")
            .IsRequired();
            
        builder.Property(e => e.ProposedProductName)
            .HasMaxLength(255)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("proposed_product_name")
            .IsRequired();
            
        builder.Property(e => e.Description)
            .HasColumnType("text")
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci");
            
        builder.Property(e => e.UnitPrice)
            .HasPrecision(12, 2)
            .HasColumnType("decimal(12,2)")
            .HasColumnName("unit_price")
            .IsRequired();
            
            
        builder.Property(e => e.EnergyEfficiencyRating)
            .HasColumnType("int")
            .HasColumnName("energy_efficiency_rating");
            
        // JSON fields - Using JsonHelpers for converter and comparer
        builder.Property(e => e.Specifications)
            .ConfigureAsJson("specifications");
            
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
            .HasPrecision(10, 3)
            .HasColumnType("decimal(10,3)")
            .HasColumnName("weight_kg");

        builder.Property(e => e.DimensionsCm)
            .ConfigureAsJson("dimensions_cm");
            
        // Enum conversion
        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasColumnType("enum('pending','approved','rejected')")
            .HasDefaultValue(ProductRegistrationStatus.Pending);
            
        builder.Property(e => e.RejectionReason)
            .HasMaxLength(500)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("rejection_reason");
            
        builder.Property(e => e.ApprovedBy)
            .HasColumnType("bigint unsigned")
            .HasColumnName("approved_by");
        
        // DateTime fields
        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at");
            
        builder.Property(e => e.ApprovedAt)
            .HasColumnType("timestamp")
            .HasColumnName("approved_at");
        
        // Indexes
        builder.HasIndex(e => new { e.VendorId, e.Status })
            .HasDatabaseName("idx_vendor_status");
            
        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("idx_created");
        
        // Foreign Keys
        builder.HasOne(e => e.Vendor)
            .WithMany()
            .HasForeignKey(e => e.VendorId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_product_registrations__vendor_id__vendor_profiles__id");
            
        builder.HasOne(e => e.Category)
            .WithMany()
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_product_registrations__category_id__product_categories__id");
            
        builder.HasOne(e => e.ApprovedByUser)
            .WithMany()
            .HasForeignKey(e => e.ApprovedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}


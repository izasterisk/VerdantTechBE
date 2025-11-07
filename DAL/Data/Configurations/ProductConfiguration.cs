using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using DAL.Data.Models;

namespace DAL.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");
        
        // Primary Key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("bigint unsigned")
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
        
        // Foreign Keys
        builder.Property(e => e.CategoryId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("category_id");

        builder.Property(e => e.VendorId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("vendor_id");
        
        // Required fields
        builder.Property(e => e.ProductCode)
            .HasMaxLength(100)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("product_code");
            
        builder.Property(e => e.ProductName)
            .HasMaxLength(255)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("product_name");
        
        // Optional string fields
        builder.Property(e => e.Slug)
            .HasMaxLength(255)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("slug");
            
        builder.Property(e => e.Description)
            .HasColumnType("text")
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("description");
            
        builder.Property(e => e.EnergyEfficiencyRating)
            .HasColumnType("int")
            .HasColumnName("energy_efficiency_rating");
        
        // Decimal fields with precision
        builder.Property(e => e.UnitPrice)
            .HasPrecision(12, 2)
            .IsRequired()
            .HasColumnName("unit_price");
            
        builder.Property(e => e.CommissionRate)
            .HasPrecision(5, 2)
            .HasDefaultValue(0.00m)
            .HasColumnName("commission_rate");
            
        builder.Property(e => e.DiscountPercentage)
            .HasPrecision(5, 2)
            .HasDefaultValue(0.00)
            .HasColumnName("discount_percentage");
            
        builder.Property(e => e.WeightKg)
            .IsRequired()
            .HasPrecision(10, 3)
            .HasColumnName("weight_kg");
            
        builder.Property(e => e.RatingAverage)
            .HasPrecision(3, 2)
            .HasDefaultValue(0.00)
            .HasColumnName("rating_average");
        
        // VARCHAR fields - Simple string properties          
        builder.Property(e => e.ManualUrls)
            .HasMaxLength(1000)
            .HasColumnName("manual_urls")
            .IsRequired(false);
            
        builder.Property(e => e.PublicUrl)
            .HasMaxLength(500)
            .HasColumnName("public_url")
            .IsRequired(false);
        
        // JSON fields - Using JsonHelpers for converter and comparer
        builder.Property(e => e.Specifications)
            .ConfigureAsJson("specifications");
            
        builder.Property(e => e.DimensionsCm)
            .ConfigureAsJson("dimensions_cm");
        
        // Integer fields
        builder.Property(e => e.WarrantyMonths)
            .HasDefaultValue(12)
            .HasColumnName("warranty_months");
            
        builder.Property(e => e.StockQuantity)
            .HasDefaultValue(0)
            .HasColumnName("stock_quantity");
            
        
        // Big integer fields
        builder.Property(e => e.ViewCount)
            .HasColumnType("bigint")
            .HasDefaultValue(0L)
            .HasColumnName("view_count");
            
        builder.Property(e => e.SoldCount)
            .HasColumnType("bigint")
            .HasDefaultValue(0L)
            .HasColumnName("sold_count");
        
        // Boolean fields
        builder.Property(e => e.IsActive)
            .HasDefaultValue(true)
            .HasColumnName("is_active");
        
        // Timestamp fields
        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at");
        
        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
            .HasColumnName("updated_at");
        
        // Foreign Key Relationships
        builder.HasOne(d => d.Category)
            .WithMany(p => p.Products)
            .HasForeignKey(d => d.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Vendor)
            .WithMany(p => p.ProductsAsVendor)
            .HasForeignKey(d => d.VendorId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Unique constraints
        builder.HasIndex(e => e.ProductCode)
            .IsUnique()
            .HasDatabaseName("idx_product_code");
            
        builder.HasIndex(e => e.Slug)
            .IsUnique()
            .HasDatabaseName("idx_slug");
        
        // Regular indexes (max 5 for large table)
        builder.HasIndex(e => e.CategoryId)
            .HasDatabaseName("idx_category");
            
        builder.HasIndex(e => e.VendorId)
            .HasDatabaseName("idx_vendor");
            
        // Full text index (updated for v7.1)
        builder.HasIndex(e => new { e.ProductName, e.Description })
            .HasAnnotation("MySql:FullTextIndex", true)
            .HasDatabaseName("idx_search");
    }
}

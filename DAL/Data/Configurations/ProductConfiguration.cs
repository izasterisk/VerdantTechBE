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
            .ValueGeneratedOnAdd();
        
        // Foreign Key
        builder.Property(e => e.CategoryId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("category_id");
        
        // Required fields
        builder.Property(e => e.ProductCode)
            .HasMaxLength(100)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("product_code");
            
        builder.Property(e => e.Name)
            .HasMaxLength(255)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci");
        
        // Optional string fields
        builder.Property(e => e.NameEn)
            .HasMaxLength(255)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("name_en");
            
        builder.Property(e => e.Slug)
            .HasMaxLength(255)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci");
            
        builder.Property(e => e.Description)
            .HasColumnType("text")
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci");
            
        builder.Property(e => e.DescriptionEn)
            .HasColumnType("text")
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("description_en");
            
        builder.Property(e => e.EnergyEfficiencyRating)
            .HasMaxLength(10)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("energy_efficiency_rating");
        
        // Decimal fields with precision
        builder.Property(e => e.Price)
            .HasPrecision(12, 2)
            .IsRequired();
            
        builder.Property(e => e.CostPrice)
            .HasPrecision(12, 2)
            .HasDefaultValue(0.00m)
            .HasColumnName("cost_price");
            
        builder.Property(e => e.CommissionRate)
            .HasPrecision(5, 2)
            .HasDefaultValue(0.00m)
            .HasColumnName("commission_rate");
            
        builder.Property(e => e.DiscountPercentage)
            .HasPrecision(5, 2)
            .HasDefaultValue(0.00)
            .HasColumnName("discount_percentage");
            
        builder.Property(e => e.WeightKg)
            .HasPrecision(10, 3)
            .HasColumnName("weight_kg");
            
        builder.Property(e => e.RatingAverage)
            .HasPrecision(3, 2)
            .HasDefaultValue(0.00)
            .HasColumnName("rating_average");
        
        // VARCHAR fields - Simple string properties
        builder.Property(e => e.GreenCertifications)
            .HasMaxLength(1000)
            .HasColumnName("green_certifications")
            .IsRequired(false);
            
        builder.Property(e => e.ManualUrls)
            .HasMaxLength(2000)
            .HasColumnName("manual_urls")
            .IsRequired(false);
            
        builder.Property(e => e.Images)
            .HasMaxLength(3000)
            .IsRequired(false);
        
        // JSON fields - Using JsonHelpers for converter and comparer
        builder.Property(e => e.Specifications)
            .HasConversion(JsonHelpers.DictionaryStringObjectConverter())
            .HasColumnType("json")
            .HasDefaultValueSql("'{}'")
            .Metadata.SetValueComparer(JsonHelpers.DictionaryStringObjectComparer());
            
        builder.Property(e => e.DimensionsCm)
            .HasConversion(JsonHelpers.DictionaryStringDecimalConverter())
            .HasColumnType("json")
            .HasDefaultValueSql("'{}'")
            .HasColumnName("dimensions_cm")
            .Metadata.SetValueComparer(JsonHelpers.DictionaryStringDecimalComparer());
        
        // Integer fields
        builder.Property(e => e.WarrantyMonths)
            .HasDefaultValue(12)
            .HasColumnName("warranty_months");
            
        builder.Property(e => e.StockQuantity)
            .HasDefaultValue(0)
            .HasColumnName("stock_quantity");
            
        builder.Property(e => e.TotalReviews)
            .HasDefaultValue(0)
            .HasColumnName("total_reviews");
        
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
        builder.Property(e => e.IsFeatured)
            .HasDefaultValue(false)
            .HasColumnName("is_featured");
            
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
        
        // Unique constraints
        builder.HasIndex(e => e.ProductCode)
            .IsUnique()
            .HasDatabaseName("idx_product_code");
            
        builder.HasIndex(e => e.Slug)
            .IsUnique()
            .HasDatabaseName("idx_slug");
        
        // Regular indexes
        builder.HasIndex(e => e.CategoryId)
            .HasDatabaseName("idx_category");
            
        builder.HasIndex(e => e.Name)
            .HasDatabaseName("idx_name");
            
        builder.HasIndex(e => e.Price)
            .HasDatabaseName("idx_price");
            
        builder.HasIndex(e => e.CommissionRate)
            .HasDatabaseName("idx_commission");
            
        builder.HasIndex(e => new { e.IsActive, e.IsFeatured })
            .HasDatabaseName("idx_active_featured");
            
        builder.HasIndex(e => e.RatingAverage)
            .HasDatabaseName("idx_rating");
        
        // Full text index
        builder.HasIndex(e => new { e.Name, e.NameEn, e.Description, e.DescriptionEn })
            .HasDatabaseName("idx_search");
    }
}

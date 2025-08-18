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
        
        // Foreign Keys
        builder.Property(e => e.VendorId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("vendor_id");
            
        builder.Property(e => e.CategoryId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("category_id");
        
        // Required fields
        builder.Property(e => e.Sku)
            .HasMaxLength(100)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci");
            
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
        
        // JSON fields - List<string> conversions
        builder.Property(e => e.GreenCertifications)
            .HasConversion(
                v => v == null || v.Count == 0 ? "[]" : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v) || v == "[]" ? new List<string>() : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("json")
            .HasDefaultValueSql("'[]'")
            .HasColumnName("green_certifications");
            
        builder.Property(e => e.Specifications)
            .HasConversion(
                v => v == null || v.Count == 0 ? "{}" : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v) || v == "{}" ? new Dictionary<string, object>() : JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("json")
            .HasDefaultValueSql("'{}'")
            .HasColumnName("specifications");
            
        builder.Property(e => e.ManualUrls)
            .HasConversion(
                v => v == null || v.Count == 0 ? "[]" : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v) || v == "[]" ? new List<string>() : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("json")
            .HasDefaultValueSql("'[]'")
            .HasColumnName("manual_urls");
            
        builder.Property(e => e.Images)
            .HasConversion(
                v => v == null || v.Count == 0 ? "[]" : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v) || v == "[]" ? new List<string>() : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("json")
            .HasDefaultValueSql("'[]'")
            .HasColumnName("images");
            
        builder.Property(e => e.DimensionsCm)
            .HasConversion(
                v => v == null || v.Count == 0 ? "{}" : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v) || v == "{}" ? new Dictionary<string, decimal>() : JsonSerializer.Deserialize<Dictionary<string, decimal>>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("json")
            .HasDefaultValueSql("'{}'")
            .HasColumnName("dimensions_cm");
        
        // Integer fields with defaults
        builder.Property(e => e.WarrantyMonths)
            .HasDefaultValue(12)
            .HasColumnName("warranty_months");
            
        builder.Property(e => e.StockQuantity)
            .HasDefaultValue(0)
            .HasColumnName("stock_quantity");
            
        builder.Property(e => e.MinOrderQuantity)
            .HasDefaultValue(1)
            .HasColumnName("min_order_quantity");
            
        builder.Property(e => e.TotalReviews)
            .HasDefaultValue(0)
            .HasColumnName("total_reviews");
        
        // Long fields with defaults
        builder.Property(e => e.ViewCount)
            .HasDefaultValue(0L)
            .HasColumnName("view_count");
            
        builder.Property(e => e.SoldCount)
            .HasDefaultValue(0L)
            .HasColumnName("sold_count");
        
        // Boolean defaults
        builder.Property(e => e.IsFeatured)
            .HasDefaultValue(false)
            .HasColumnName("is_featured");
            
        builder.Property(e => e.IsActive)
            .HasDefaultValue(true)
            .HasColumnName("is_active");
        
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
        builder.HasOne(d => d.Vendor)
            .WithMany(p => p.Products)
            .HasForeignKey(d => d.VendorId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(d => d.Category)
            .WithMany(p => p.Products)
            .HasForeignKey(d => d.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Indexes
        builder.HasIndex(e => e.VendorId)
            .HasDatabaseName("idx_vendor");
            
        builder.HasIndex(e => e.CategoryId)
            .HasDatabaseName("idx_category");
            
        builder.HasIndex(e => e.Sku)
            .IsUnique()
            .HasDatabaseName("idx_sku");
            
        builder.HasIndex(e => e.Name)
            .HasDatabaseName("idx_name");
            
        builder.HasIndex(e => e.Price)
            .HasDatabaseName("idx_price");
            
        builder.HasIndex(e => new { e.IsActive, e.IsFeatured })
            .HasDatabaseName("idx_active_featured");
            
        builder.HasIndex(e => e.RatingAverage)
            .HasDatabaseName("idx_rating");
        
        // Full-text search index
        builder.HasIndex(e => new { e.Name, e.NameEn, e.Description, e.DescriptionEn })
            .HasAnnotation("MySql:FullTextIndex", true)
            .HasDatabaseName("idx_search");
    }
}

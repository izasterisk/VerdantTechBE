using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using DAL.Data.Models;
using DAL.Data;

namespace DAL.Data.Configurations;

public class ProductReviewConfiguration : IEntityTypeConfiguration<ProductReview>
{
    public void Configure(EntityTypeBuilder<ProductReview> builder)
    {
        builder.ToTable("product_reviews");
        
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
            .IsRequired()
            .HasColumnName("order_id");
            
        builder.Property(e => e.CustomerId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("customer_id");
        
        // Rating with check constraint equivalent
        builder.Property(e => e.Rating)
            .IsRequired();
        
        // Optional string fields
        builder.Property(e => e.Title)
            .HasMaxLength(255)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci");
            
        builder.Property(e => e.Comment)
            .HasColumnType("text")
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci");
        
        // JSON field for images array
        builder.Property(e => e.Images)
            .HasConversion(
                v => v == null || v.Count == 0 ? "[]" : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v) || v == "[]" ? new List<string>() : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("json");
        
        // Count fields with defaults
        builder.Property(e => e.HelpfulCount)
            .HasDefaultValue(0)
            .HasColumnName("helpful_count");
            
        builder.Property(e => e.UnhelpfulCount)
            .HasDefaultValue(0)
            .HasColumnName("unhelpful_count");
        
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
        
        // Relationship with Product
        builder.HasOne(d => d.Product)
            .WithMany(p => p.ProductReviews)
            .HasForeignKey(d => d.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Relationship with Order
        builder.HasOne(d => d.Order)
            .WithMany(p => p.ProductReviews)
            .HasForeignKey(d => d.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Relationship with User (customer/reviewer)
        builder.HasOne(d => d.Customer)
            .WithMany(p => p.ProductReviews)
            .HasForeignKey(d => d.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Unique constraint - one review per product per order per customer
        builder.HasIndex(e => new { e.ProductId, e.OrderId, e.CustomerId })
            .IsUnique()
            .HasDatabaseName("unique_product_order_customer");
        
        // Indexes
        builder.HasIndex(e => new { e.ProductId, e.Rating })
            .HasDatabaseName("idx_product_rating");
            
        builder.HasIndex(e => e.CustomerId)
            .HasDatabaseName("idx_customer");
            
        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("idx_created_at");
    }
}

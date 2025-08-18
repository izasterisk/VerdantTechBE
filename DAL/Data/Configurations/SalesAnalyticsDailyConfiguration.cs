using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using DAL.Data.Models;
using DAL.Data;

namespace DAL.Data.Configurations;

public class SalesAnalyticsDailyConfiguration : IEntityTypeConfiguration<SalesAnalyticsDaily>
{
    public void Configure(EntityTypeBuilder<SalesAnalyticsDaily> builder)
    {
        builder.ToTable("sales_analytics_daily");
        
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
        
        // Date field
        builder.Property(e => e.Date)
            .HasColumnType("date")
            .IsRequired();
        
        // Integer fields with defaults
        builder.Property(e => e.TotalOrders)
            .HasDefaultValue(0)
            .HasColumnName("total_orders");
            
        builder.Property(e => e.TotalProductsSold)
            .HasDefaultValue(0)
            .HasColumnName("total_products_sold");
            
        builder.Property(e => e.NewCustomers)
            .HasDefaultValue(0)
            .HasColumnName("new_customers");
            
        builder.Property(e => e.ReturningCustomers)
            .HasDefaultValue(0)
            .HasColumnName("returning_customers");
        
        // Decimal fields with precision
        builder.Property(e => e.TotalRevenue)
            .HasPrecision(15, 2)
            .HasDefaultValue(0.00m)
            .HasColumnName("total_revenue");
            
        builder.Property(e => e.AverageOrderValue)
            .HasPrecision(12, 2)
            .HasDefaultValue(0.00m)
            .HasColumnName("average_order_value");
        
        // JSON field for top products
        builder.Property(e => e.TopProducts)
            .HasConversion(
                v => v == null || v.Count == 0 ? "[]" : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v) || v == "[]" ? new List<Dictionary<string, object>>() : JsonSerializer.Deserialize<List<Dictionary<string, object>>>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("json")
            .HasColumnName("top_products");
        
        // DateTime field
        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at");
        
        // Foreign Key Relationships
        builder.HasOne(d => d.Vendor)
            .WithMany(p => p.SalesAnalytics)
            .HasForeignKey(d => d.VendorId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Unique constraint - one record per date per vendor
        builder.HasIndex(e => new { e.Date, e.VendorId })
            .IsUnique()
            .HasDatabaseName("unique_date_vendor");
        
        // Indexes
        builder.HasIndex(e => e.Date)
            .HasDatabaseName("idx_date");
            
        builder.HasIndex(e => e.VendorId)
            .HasDatabaseName("idx_vendor");
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAL.Data.Models;

namespace DAL.Data.Configurations;

public class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.ToTable("product_categories");
        
        // Primary Key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("bigint unsigned")
            .ValueGeneratedOnAdd();
        
        // Foreign Key to self (parent_id)
        builder.Property(e => e.ParentId)
            .HasColumnType("bigint unsigned")
            .HasColumnName("parent_id");
        
        // Required fields
        builder.Property(e => e.Name)
            .HasMaxLength(255)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci");
            
        builder.Property(e => e.Slug)
            .HasMaxLength(255)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci");
        
        // Optional fields
        builder.Property(e => e.NameEn)
            .HasMaxLength(255)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("name_en");
            
        builder.Property(e => e.Description)
            .HasColumnType("text")
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci");
            
        builder.Property(e => e.IconUrl)
            .HasMaxLength(500)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("icon_url");
        
        // Integer fields with defaults
        builder.Property(e => e.SortOrder)
            .HasDefaultValue(0)
            .HasColumnName("sort_order");
        
        // Boolean defaults
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
        
        // Self-referencing foreign key relationship
        builder.HasOne(d => d.Parent)
            .WithMany(p => p.InverseParent)
            .HasForeignKey(d => d.ParentId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indexes
        builder.HasIndex(e => e.ParentId)
            .HasDatabaseName("idx_parent");
            
        builder.HasIndex(e => e.Slug)
            .IsUnique()
            .HasDatabaseName("idx_slug");
            
        builder.HasIndex(e => new { e.IsActive, e.SortOrder })
            .HasDatabaseName("idx_active_sort");
    }
}

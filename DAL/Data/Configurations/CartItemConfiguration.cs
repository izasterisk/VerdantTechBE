using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAL.Data.Models;

namespace DAL.Data.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("cart_items");

        // Primary Key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("bigint unsigned")
            .ValueGeneratedOnAdd();

        // Foreign Keys
        builder.Property(e => e.CartId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("cart_id");

        builder.Property(e => e.ProductId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("product_id");

        // Required fields
        builder.Property(e => e.Quantity)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(e => e.UnitPrice)
            .HasColumnType("decimal(12,2)")
            .IsRequired()
            .HasColumnName("unit_price");

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
        builder.HasOne(d => d.Cart)
            .WithMany(p => p.CartItems)
            .HasForeignKey(d => d.CartId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Product)
            .WithMany(p => p.CartItems)
            .HasForeignKey(d => d.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint
        builder.HasIndex(e => new { e.CartId, e.ProductId })
            .IsUnique()
            .HasDatabaseName("unique_cart_product");

        // Indexes
        builder.HasIndex(e => e.CartId)
            .HasDatabaseName("idx_cart");

        builder.HasIndex(e => e.ProductId)
            .HasDatabaseName("idx_product");

        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("idx_created_at");
    }
}

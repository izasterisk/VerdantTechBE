using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAL.Data.Models;

namespace DAL.Data.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("cart");
        
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
            
        builder.Property(c => c.UserId)
            .HasColumnName("user_id")
            .IsRequired();
            
        builder.Property(c => c.ProductId)
            .HasColumnName("product_id")
            .IsRequired();
            
        builder.Property(c => c.Quantity)
            .HasColumnName("quantity")
            .HasDefaultValue(1)
            .IsRequired();
            
        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
        
        // Foreign Key constraints
        builder.HasOne(c => c.User)
            .WithMany(u => u.Carts)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(c => c.Product)
            .WithMany(p => p.Carts)
            .HasForeignKey(c => c.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Unique constraint
        builder.HasIndex(c => new { c.UserId, c.ProductId })
            .IsUnique()
            .HasDatabaseName("unique_user_product");
            
        // Additional indexes
        builder.HasIndex(c => c.UserId)
            .HasDatabaseName("idx_user");
            
        builder.HasIndex(c => c.ProductId)
            .HasDatabaseName("idx_product");
    }
}

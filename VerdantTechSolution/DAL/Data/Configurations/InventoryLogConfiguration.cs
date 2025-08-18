using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAL.Data.Models;
using DAL.Data;

namespace DAL.Data.Configurations;

public class InventoryLogConfiguration : IEntityTypeConfiguration<InventoryLog>
{
    public void Configure(EntityTypeBuilder<InventoryLog> builder)
    {
        builder.ToTable("inventory_logs");
        
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
            
        builder.Property(e => e.ReferenceId)
            .HasColumnType("bigint unsigned")
            .HasColumnName("reference_id");
            
        builder.Property(e => e.CreatedBy)
            .HasColumnType("bigint unsigned")
            .HasColumnName("created_by");
        
        // Enum conversion for type
        builder.Property(e => e.Type)
            .HasConversion<string>()
            .HasColumnType("enum('in','out','adjustment')")
            .IsRequired();
        
        // Integer fields
        builder.Property(e => e.Quantity)
            .IsRequired();
            
        builder.Property(e => e.BalanceAfter)
            .IsRequired()
            .HasColumnName("balance_after");
        
        // String fields
        builder.Property(e => e.Reason)
            .HasMaxLength(255)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci");
            
        builder.Property(e => e.ReferenceType)
            .HasMaxLength(50)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("reference_type");
        
        // DateTime field
        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at");
        
        // Foreign Key Relationships
        builder.HasOne(d => d.Product)
            .WithMany(p => p.InventoryLogs)
            .HasForeignKey(d => d.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(d => d.CreatedByNavigation)
            .WithMany(p => p.InventoryLogs)
            .HasForeignKey(d => d.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull);
        
        // Indexes
        builder.HasIndex(e => new { e.ProductId, e.Type })
            .HasDatabaseName("idx_product_type");
            
        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("idx_created_at");
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAL.Data.Models;

namespace DAL.Data.Configurations;

public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.ToTable("wallets");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("bigint unsigned")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.VendorId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("vendor_id");

        builder.Property(e => e.Balance)
            .HasPrecision(12, 2)
            .HasDefaultValue(0.00m)
            .HasColumnName("balance");

        builder.Property(e => e.LastUpdatedBy)
            .HasColumnType("bigint unsigned")
            .HasColumnName("last_updated_by");

        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at");

        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
            .HasColumnName("updated_at");

        builder.HasOne(d => d.Vendor)
            .WithOne(p => p.WalletAsVendor)
            .HasForeignKey<Wallet>(d => d.VendorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.LastUpdatedByNavigation)
            .WithMany()
            .HasForeignKey(d => d.LastUpdatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.VendorId)
            .HasDatabaseName("idx_vendor");
    }
}



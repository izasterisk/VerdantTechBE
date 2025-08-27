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

        builder.Property(e => e.PendingWithdraw)
            .HasPrecision(12, 2)
            .HasDefaultValue(0.00m)
            .HasColumnName("pending_withdraw");

        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at");

        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
            .HasColumnName("updated_at");

        builder.HasOne(d => d.VendorProfile)
            .WithOne(p => p.Wallet)
            .HasForeignKey<Wallet>(d => d.VendorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.VendorId)
            .IsUnique();
    }
}



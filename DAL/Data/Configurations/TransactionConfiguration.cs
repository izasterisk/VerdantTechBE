using DAL.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasColumnType("bigint unsigned")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Type)
            .HasConversion<string>()
            .HasColumnName("type")
            .HasColumnType("enum('customer_payment','vendor_commission','refund','payout','system_fee','adjustment')")
            .IsRequired();

        builder.Property(e => e.Amount)
            .HasColumnName("amount")
            .HasColumnType("decimal(12,2)")
            .IsRequired();

        builder.Property(e => e.Description)
            .HasColumnName("description")
            .HasColumnType("varchar(255)")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.ReferenceType)
            .HasColumnName("reference_type")
            .HasColumnType("varchar(50)")
            .HasMaxLength(50);

        builder.Property(e => e.ReferenceId)
            .HasColumnName("reference_id")
            .HasColumnType("bigint unsigned");

        builder.Property(e => e.FromWalletId)
            .HasColumnName("from_wallet_id")
            .HasColumnType("bigint unsigned");

        builder.Property(e => e.ToWalletId)
            .HasColumnName("to_wallet_id")
            .HasColumnType("bigint unsigned");

        builder.Property(e => e.BalanceBefore)
            .HasColumnName("balance_before")
            .HasColumnType("decimal(12,2)")
            .HasDefaultValue(0.00m);

        builder.Property(e => e.BalanceAfter)
            .HasColumnName("balance_after")
            .HasColumnType("decimal(12,2)")
            .HasDefaultValue(0.00m);

        builder.Property(e => e.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("bigint unsigned")
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Foreign keys
        builder.HasOne(e => e.FromWallet)
            .WithMany()
            .HasForeignKey(e => e.FromWalletId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.ToWallet)
            .WithMany()
            .HasForeignKey(e => e.ToWalletId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.CreatedByNavigation)
            .WithMany()
            .HasForeignKey(e => e.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(e => e.Type).HasDatabaseName("idx_type");
        builder.HasIndex(e => e.FromWalletId).HasDatabaseName("idx_from_wallet");
        builder.HasIndex(e => e.ToWalletId).HasDatabaseName("idx_to_wallet");
        builder.HasIndex(e => new { e.ReferenceType, e.ReferenceId }).HasDatabaseName("idx_reference");
        builder.HasIndex(e => e.CreatedBy).HasDatabaseName("idx_created_by");
        builder.HasIndex(e => e.CreatedAt).HasDatabaseName("idx_created_at");
    }
}

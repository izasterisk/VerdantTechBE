using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAL.Data.Models;
using DAL.Data;

namespace DAL.Data.Configurations;

public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
{
    public void Configure(EntityTypeBuilder<WalletTransaction> builder)
    {
        builder.ToTable("wallet_transactions");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("bigint unsigned")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.WalletId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("wallet_id");

        builder.Property(e => e.Type)
            .HasConversion<string>()
            .HasColumnType("enum('credit','debit')")
            .IsRequired()
            .HasColumnName("type");

        builder.Property(e => e.Amount)
            .HasPrecision(12, 2)
            .IsRequired()
            .HasColumnName("amount");

        builder.Property(e => e.ReferenceType)
            .HasMaxLength(50)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("reference_type");

        builder.Property(e => e.ReferenceId)
            .HasColumnType("bigint unsigned")
            .HasColumnName("reference_id");

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasColumnType("enum('pending','completed','failed')")
            .HasDefaultValue(WalletTransactionStatus.Pending)
            .HasColumnName("status");

        builder.Property(e => e.Description)
            .HasMaxLength(255)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("description");

        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at");

        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
            .HasColumnName("updated_at");

        builder.HasOne(d => d.Wallet)
            .WithMany(p => p.WalletTransactions)
            .HasForeignKey(d => d.WalletId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.WalletId).HasDatabaseName("idx_wallet");
        builder.HasIndex(e => new { e.WalletId, e.CreatedAt }).HasDatabaseName("idx_wallet_created");
        builder.HasIndex(e => e.Status).HasDatabaseName("idx_status");
    }
}



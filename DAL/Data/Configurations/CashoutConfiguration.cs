using DAL.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class CashoutConfiguration : IEntityTypeConfiguration<Cashout>
{
    public void Configure(EntityTypeBuilder<Cashout> builder)
    {
        builder.ToTable("cashouts");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasColumnType("bigint unsigned")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.TransactionId)
            .HasColumnName("transaction_id")
            .HasColumnType("bigint unsigned");

        builder.Property(e => e.RecipientWalletId)
            .HasColumnName("recipient_wallet_id")
            .HasColumnType("bigint unsigned");

        builder.Property(e => e.Amount)
            .HasColumnName("amount")
            .HasColumnType("decimal(12,2)")
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasColumnName("status")
            .HasColumnType("enum('pending','processing','succeeded','failed')")
            .HasDefaultValue(PayoutStatus.Pending);

        builder.Property(e => e.BankTransactionId)
            .HasColumnName("bank_transaction_id")
            .HasColumnType("varchar(255)")
            .HasMaxLength(255);

        builder.Property(e => e.BankingDetails)
            .HasColumnName("banking_details")
            .HasColumnType("json");

        builder.Property(e => e.ReferenceType)
            .HasColumnName("reference_type")
            .HasColumnType("varchar(50)")
            .HasMaxLength(50);

        builder.Property(e => e.ReferenceId)
            .HasColumnName("reference_id")
            .HasColumnType("bigint unsigned");

        builder.Property(e => e.Notes)
            .HasColumnName("notes")
            .HasColumnType("text");

        builder.Property(e => e.ProcessedAt)
            .HasColumnName("processed_at")
            .HasColumnType("timestamp");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

        // Foreign keys
        builder.HasOne(e => e.Transaction)
            .WithMany(t => t.Cashouts)
            .HasForeignKey(e => e.TransactionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.RecipientWallet)
            .WithMany()
            .HasForeignKey(e => e.RecipientWalletId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(e => e.TransactionId).HasDatabaseName("idx_transaction");
        builder.HasIndex(e => e.RecipientWalletId).HasDatabaseName("idx_recipient_wallet");
        builder.HasIndex(e => e.Status).HasDatabaseName("idx_status");
        builder.HasIndex(e => new { e.ReferenceType, e.ReferenceId }).HasDatabaseName("idx_reference");
        builder.HasIndex(e => e.CreatedAt).HasDatabaseName("idx_created_at");
    }
}

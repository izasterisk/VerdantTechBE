using DAL.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

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

        builder.Property(e => e.TransactionType)
            .HasConversion(
                v => v.ToString()
                    .ToLowerInvariant()
                    .Replace("paymentin", "payment_in")
                    .Replace("walletcredit", "wallet_credit")
                    .Replace("walletdebit", "wallet_debit"),
                v => Enum.Parse<TransactionType>(v
                    .Replace("payment_in", "PaymentIn")
                    .Replace("wallet_credit", "WalletCredit")
                    .Replace("wallet_debit", "WalletDebit"), true))
            .HasColumnName("transaction_type")
            .HasColumnType("enum('payment_in','cashout','wallet_credit','wallet_debit','commission','refund','adjustment')")
            .IsRequired();

        builder.Property(e => e.Amount)
            .HasColumnName("amount")
            .HasColumnType("decimal(12,2)")
            .IsRequired();

        builder.Property(e => e.Currency)
            .HasColumnName("currency")
            .HasColumnType("varchar(3)")
            .HasMaxLength(3)
            .HasDefaultValue("VND");

        // Core references
        builder.Property(e => e.OrderId)
            .HasColumnName("order_id")
            .HasColumnType("bigint unsigned");

        builder.Property(e => e.CustomerId)
            .HasColumnName("customer_id")
            .HasColumnType("bigint unsigned");

        builder.Property(e => e.VendorId)
            .HasColumnName("vendor_id")
            .HasColumnType("bigint unsigned");

        // Wallet related fields
        builder.Property(e => e.WalletId)
            .HasColumnName("wallet_id")
            .HasColumnType("bigint unsigned");

        builder.Property(e => e.BalanceBefore)
            .HasColumnName("balance_before")
            .HasColumnType("decimal(12,2)");

        builder.Property(e => e.BalanceAfter)
            .HasColumnName("balance_after")
            .HasColumnType("decimal(12,2)");

        // Status and metadata
        builder.Property(e => e.Status)
            .HasConversion(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<TransactionStatus>(v, true))
            .HasColumnName("status")
            .HasColumnType("enum('pending','completed','failed','cancelled')")
            .HasDefaultValue(TransactionStatus.Pending);

        builder.Property(e => e.Description)
            .HasColumnName("description")
            .HasColumnType("varchar(255)")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.Metadata)
            .HasConversion(JsonHelpers.DictionaryStringObjectConverter())
            .HasColumnType("json")
            .HasColumnName("metadata")
            .Metadata.SetValueComparer(JsonHelpers.DictionaryStringObjectComparer());

        // Reference to domain-specific tables
        builder.Property(e => e.ReferenceType)
            .HasColumnName("reference_type")
            .HasColumnType("varchar(50)")
            .HasMaxLength(50);

        builder.Property(e => e.ReferenceId)
            .HasColumnName("reference_id")
            .HasColumnType("bigint unsigned");

        // Audit fields
        builder.Property(e => e.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("bigint unsigned");

        builder.Property(e => e.ProcessedBy)
            .HasColumnName("processed_by")
            .HasColumnType("bigint unsigned");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.CompletedAt)
            .HasColumnName("completed_at")
            .HasColumnType("timestamp");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

        // Foreign keys
        builder.HasOne(e => e.Order)
            .WithMany(o => o.Transactions)
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Customer)
            .WithMany()
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Vendor)
            .WithMany(v => v.Transactions)
            .HasForeignKey(e => e.VendorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Wallet)
            .WithMany(w => w.Transactions)
            .HasForeignKey(e => e.WalletId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CreatedByNavigation)
            .WithMany(u => u.TransactionsCreated)
            .HasForeignKey(e => e.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ProcessedByNavigation)
            .WithMany(u => u.TransactionsProcessed)
            .HasForeignKey(e => e.ProcessedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(e => new { e.TransactionType, e.Status }).HasDatabaseName("idx_type_status");
        builder.HasIndex(e => e.OrderId).HasDatabaseName("idx_order");
        builder.HasIndex(e => e.CustomerId).HasDatabaseName("idx_customer");
        builder.HasIndex(e => e.VendorId).HasDatabaseName("idx_vendor");
        builder.HasIndex(e => e.WalletId).HasDatabaseName("idx_wallet");
        builder.HasIndex(e => e.CreatedAt).HasDatabaseName("idx_created_at");
        builder.HasIndex(e => e.CompletedAt).HasDatabaseName("idx_completed_at");
        builder.HasIndex(e => new { e.ReferenceType, e.ReferenceId }).HasDatabaseName("idx_reference");
        builder.HasIndex(e => e.Amount).HasDatabaseName("idx_amount");
        builder.HasIndex(e => e.CreatedBy).HasDatabaseName("idx_created_by");
    }
}

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

        builder.Property(e => e.OrderId)
            .HasColumnName("order_id")
            .HasColumnType("bigint unsigned");

        builder.Property(e => e.UserId)
            .HasColumnName("user_id")
            .HasColumnType("bigint unsigned")
            .IsRequired();

        // Status and metadata
        builder.Property(e => e.Status)
            .HasConversion(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<TransactionStatus>(v, true))
            .HasColumnName("status")
            .HasColumnType("enum('pending','completed','failed','cancelled')")
            .HasDefaultValue(TransactionStatus.Pending);

        builder.Property(e => e.Note)
            .HasColumnName("note")
            .HasColumnType("varchar(255)")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.GatewayPaymentId)
            .HasColumnName("gateway_payment_id")
            .HasColumnType("varchar(255)")
            .HasMaxLength(255);

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

        builder.HasOne(e => e.User)
            .WithMany(u => u.TransactionsAsUser)
            .HasForeignKey(e => e.UserId)
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
        builder.HasIndex(e => e.UserId).HasDatabaseName("idx_user");
        builder.HasIndex(e => new { e.TransactionType, e.Status }).HasDatabaseName("idx_type_status");
        builder.HasIndex(e => e.OrderId).HasDatabaseName("idx_order");
        builder.HasIndex(e => e.GatewayPaymentId).HasDatabaseName("idx_gateway_payment");
        builder.HasIndex(e => e.CreatedAt).HasDatabaseName("idx_created");
    }
}

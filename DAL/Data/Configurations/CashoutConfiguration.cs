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

        builder.Property(e => e.VendorId)
            .HasColumnName("vendor_id")
            .HasColumnType("bigint unsigned")
            .IsRequired();

        builder.Property(e => e.TransactionId)
            .HasColumnName("transaction_id")
            .HasColumnType("bigint unsigned");

        builder.Property(e => e.BankAccountId)
            .HasColumnName("bank_account_id")
            .HasColumnType("bigint unsigned")
            .IsRequired();

        builder.Property(e => e.Amount)
            .HasColumnName("amount")
            .HasColumnType("decimal(12,2)")
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<CashoutStatus>(v, true))
            .HasColumnName("status")
            .HasColumnType("enum('pending','processing','completed','failed','cancelled')")
            .HasDefaultValue(CashoutStatus.Pending);

        builder.Property(e => e.Reason)
            .HasColumnName("reason")
            .HasColumnType("varchar(255)")
            .HasMaxLength(255);

        builder.Property(e => e.GatewayTransactionId)
            .HasColumnName("gateway_transaction_id")
            .HasColumnType("varchar(255)")
            .HasMaxLength(255);

        builder.Property(e => e.ReferenceType)
            .HasColumnName("reference_type")
            .HasColumnType("varchar(50)")
            .HasMaxLength(50);

        builder.Property(e => e.ReferenceId)
            .HasColumnName("reference_id")
            .HasColumnType("bigint unsigned");

        builder.Property(e => e.Notes)
            .HasColumnName("notes")
            .HasColumnType("varchar(500)")
            .HasMaxLength(500);

        builder.Property(e => e.ProcessedBy)
            .HasColumnName("processed_by")
            .HasColumnType("bigint unsigned");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.ProcessedAt)
            .HasColumnName("processed_at")
            .HasColumnType("timestamp");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

        // Foreign keys
        builder.HasOne(e => e.Vendor)
            .WithMany(v => v.CashoutsAsVendor)
            .HasForeignKey(e => e.VendorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.BankAccount)
            .WithMany(b => b.Cashouts)
            .HasForeignKey(e => e.BankAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Transaction)
            .WithMany(t => t.Cashouts)
            .HasForeignKey(e => e.TransactionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ProcessedByNavigation)
            .WithMany(u => u.CashoutsProcessed)
            .HasForeignKey(e => e.ProcessedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(e => e.GatewayTransactionId)
            .IsUnique()
            .HasDatabaseName("idx_unique_gateway_transaction");

        builder.HasIndex(e => e.VendorId).HasDatabaseName("idx_vendor");
        builder.HasIndex(e => e.TransactionId).HasDatabaseName("idx_transaction");
        builder.HasIndex(e => e.Status).HasDatabaseName("idx_status");
        builder.HasIndex(e => e.TransactionId).HasDatabaseName("idx_transaction");
        builder.HasIndex(e => e.ProcessedAt).HasDatabaseName("idx_processed");
        builder.HasIndex(e => new { e.ReferenceType, e.ReferenceId }).HasDatabaseName("idx_reference");
    }
}

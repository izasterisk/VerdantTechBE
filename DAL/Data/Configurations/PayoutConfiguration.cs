using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAL.Data.Models;
using DAL.Data;

namespace DAL.Data.Configurations;

public class PayoutConfiguration : IEntityTypeConfiguration<Payout>
{
    public void Configure(EntityTypeBuilder<Payout> builder)
    {
        builder.ToTable("payouts");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("bigint unsigned")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.VendorId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("vendor_id");

        builder.Property(e => e.Amount)
            .HasPrecision(12, 2)
            .IsRequired()
            .HasColumnName("amount");

        builder.Property(e => e.BankCode)
            .HasMaxLength(20)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("bank_code");

        builder.Property(e => e.BankAccountNumber)
            .HasMaxLength(50)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("bank_account_number");

        builder.Property(e => e.BankAccountHolder)
            .HasMaxLength(255)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("bank_account_holder");

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasColumnType("enum('pending','processing','succeeded','failed')")
            .HasDefaultValue(PayoutStatus.Pending)
            .HasColumnName("status");

        builder.Property(e => e.TransactionId)
            .HasMaxLength(255)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("transaction_id");

        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at");

        builder.Property(e => e.RequestedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("requested_at");

        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
            .HasColumnName("updated_at");

        // Relationships
        builder.HasOne(d => d.VendorProfile)
            .WithMany(p => p.Payouts)
            .HasForeignKey(d => d.VendorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship to SupportedBank by bank_code (alternate key)
        builder.HasOne(d => d.Bank)
            .WithMany(p => p.Payouts)
            .HasPrincipalKey(b => b.BankCode)
            .HasForeignKey(d => d.BankCode)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.TransactionId)
            .IsUnique()
            .HasDatabaseName("idx_unique_transaction");

        builder.HasIndex(e => e.VendorId).HasDatabaseName("idx_vendor");
        builder.HasIndex(e => e.Status).HasDatabaseName("idx_status");
        builder.HasIndex(e => e.RequestedAt).HasDatabaseName("idx_requested");
    }
}



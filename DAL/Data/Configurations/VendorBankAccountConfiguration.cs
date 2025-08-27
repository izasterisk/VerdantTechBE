using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAL.Data.Models;

namespace DAL.Data.Configurations;

public class VendorBankAccountConfiguration : IEntityTypeConfiguration<VendorBankAccount>
{
    public void Configure(EntityTypeBuilder<VendorBankAccount> builder)
    {
        builder.ToTable("vendor_bank_accounts");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("bigint unsigned")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.VendorId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("vendor_id");

        builder.Property(e => e.BankId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("bank_id");

        builder.Property(e => e.AccountNumber)
            .HasMaxLength(50)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("account_number");

        builder.Property(e => e.AccountHolder)
            .HasMaxLength(255)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("account_holder");

        builder.Property(e => e.IsDefault)
            .HasDefaultValue(false)
            .HasColumnName("is_default");

        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at");

        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
            .HasColumnName("updated_at");

        builder.HasOne(d => d.VendorProfile)
            .WithMany(p => p.VendorBankAccounts)
            .HasForeignKey(d => d.VendorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Bank)
            .WithMany(p => p.VendorBankAccounts)
            .HasForeignKey(d => d.BankId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.VendorId, e.BankId, e.AccountNumber })
            .IsUnique()
            .HasDatabaseName("unique_vendor_bank_account");

        builder.HasIndex(e => e.VendorId).HasDatabaseName("idx_vendor");
        builder.HasIndex(e => e.BankId).HasDatabaseName("idx_bank");
        builder.HasIndex(e => new { e.VendorId, e.IsDefault }).HasDatabaseName("idx_vendor_default");
    }
}



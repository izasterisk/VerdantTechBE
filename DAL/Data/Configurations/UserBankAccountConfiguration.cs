using DAL.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class UserBankAccountConfiguration : IEntityTypeConfiguration<UserBankAccount>
{
    public void Configure(EntityTypeBuilder<UserBankAccount> builder)
    {
        builder.ToTable("user_bank_accounts");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasColumnType("bigint unsigned")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.UserId)
            .HasColumnName("user_id")
            .HasColumnType("bigint unsigned")
            .IsRequired();

        builder.Property(e => e.BankCode)
            .HasColumnName("bank_code")
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.AccountNumber)
            .HasColumnName("account_number")
            .HasColumnType("varchar(50)")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.AccountHolder)
            .HasColumnName("account_holder")
            .HasColumnType("varchar(255)")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

        // Foreign Key
        builder.HasOne(e => e.User)
            .WithMany(u => u.UserBankAccounts)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint
        builder.HasIndex(e => new { e.UserId, e.AccountNumber })
            .IsUnique()
            .HasDatabaseName("unique_user_bank_account");

        // Index
        builder.HasIndex(e => e.UserId).HasDatabaseName("idx_user");
    }
}

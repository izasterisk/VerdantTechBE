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
            .HasColumnType("bigint unsigned")
            .IsRequired();

        builder.Property(e => e.ReferenceType)
            .HasConversion(
                v => v.ToString().ToLowerInvariant().Replace("withdrawal", "_withdrawal").Replace("adjustment", "_adjustment"),
                v => Enum.Parse<CashoutReferenceType>(v.Replace("_", ""), true))
            .HasColumnName("reference_type")
            .HasColumnType("enum('vendor_withdrawal','refund','admin_adjustment')")
            .IsRequired();

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
        builder.HasOne(e => e.Transaction)
            .WithMany(t => t.Cashouts)
            .HasForeignKey(e => e.TransactionId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasOne(e => e.ProcessedByNavigation)
            .WithMany(u => u.CashoutsProcessed)
            .HasForeignKey(e => e.ProcessedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(e => e.TransactionId).HasDatabaseName("idx_transaction");
        builder.HasIndex(e => new { e.ReferenceType, e.ReferenceId }).HasDatabaseName("idx_reference");
        builder.HasIndex(e => new { e.ProcessedBy, e.ProcessedAt }).HasDatabaseName("idx_processed");
    }
}

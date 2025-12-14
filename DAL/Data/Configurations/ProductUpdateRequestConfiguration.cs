using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAL.Data.Models;

namespace DAL.Data.Configurations;

public class ProductUpdateRequestConfiguration : IEntityTypeConfiguration<ProductUpdateRequest>
{
    public void Configure(EntityTypeBuilder<ProductUpdateRequest> builder)
    {
        builder.ToTable("product_update_requests");

        // PK
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("bigint unsigned")
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // Required FKs
        builder.Property(e => e.ProductSnapshotId)
            .HasColumnType("bigint unsigned")
            .HasColumnName("product_snapshot_id")
            .IsRequired();

        builder.Property(e => e.ProductId)
            .HasColumnType("bigint unsigned")
            .HasColumnName("product_id")
            .IsRequired();

        // Enum Status
        builder.Property(e => e.Status)
            .HasConversion(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<ProductRegistrationStatus>(v, true))
            .HasColumnType("enum('pending','approved','rejected')")
            .HasColumnName("status")
            .HasDefaultValue(ProductRegistrationStatus.Pending)
            .IsRequired();

        builder.Property(e => e.RejectionReason)
            .HasMaxLength(500)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("rejection_reason");

        builder.Property(e => e.ProcessedBy)
            .HasColumnType("bigint unsigned")
            .HasColumnName("processed_by");

        // Timestamps
        builder.Property(e => e.ProcessedAt)
            .HasColumnType("timestamp")
            .HasColumnName("processed_at");

        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at");

        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp")
            .ValueGeneratedOnAddOrUpdate()
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("updated_at");

        // Indexes
        builder.HasIndex(e => e.ProductSnapshotId).HasDatabaseName("idx_product_snapshot");
        builder.HasIndex(e => e.ProductId).HasDatabaseName("idx_product");
        builder.HasIndex(e => e.Status).HasDatabaseName("idx_status");

        // Foreign Keys
        builder.HasOne(e => e.ProductSnapshot)
            .WithMany(p => p.ProductUpdateRequests)
            .HasForeignKey(e => e.ProductSnapshotId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("product_update_requests_ibfk_1");

        builder.HasOne(e => e.Product)
            .WithMany()
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("product_update_requests_ibfk_2");

        builder.HasOne(e => e.ProcessedByUser)
            .WithMany()
            .HasForeignKey(e => e.ProcessedBy)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("product_update_requests_ibfk_3");
    }
}

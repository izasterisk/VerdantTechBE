using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAL.Data.Models;

namespace DAL.Data.Configurations;

public class ProductCertificateConfiguration : IEntityTypeConfiguration<ProductCertificate>
{
    public void Configure(EntityTypeBuilder<ProductCertificate> builder)
    {
        builder.ToTable("product_certificates");

        // Primary Key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("bigint unsigned")
            .ValueGeneratedOnAdd();

        // Foreign Keys
        builder.Property(e => e.ProductId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("product_id");

        // Required fields
        builder.Property(e => e.CertificationCode)
            .HasMaxLength(50)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("certification_code");

        builder.Property(e => e.CertificationName)
            .HasMaxLength(255)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("certification_name");

        builder.Property(e => e.CertificateUrl)
            .HasMaxLength(500)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("certificate_url");

        builder.Property(e => e.PublicUrl)
            .HasMaxLength(500)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("public_url");

        // Enum conversion for status
        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasColumnType("enum('pending','verified','rejected')")
            .HasDefaultValue(ProductCertificateStatus.Pending);

        // Optional fields
        builder.Property(e => e.RejectionReason)
            .HasMaxLength(500)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("rejection_reason");

        builder.Property(e => e.VerifiedBy)
            .HasColumnType("bigint unsigned")
            .HasColumnName("verified_by");

        // DateTime fields
        builder.Property(e => e.UploadedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("uploaded_at");

        builder.Property(e => e.VerifiedAt)
            .HasColumnType("timestamp")
            .HasColumnName("verified_at");

        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at");

        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
            .HasColumnName("updated_at");

        // Foreign Key Relationships
        builder.HasOne(d => d.Product)
            .WithMany(p => p.ProductCertificates)
            .HasForeignKey(d => d.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.VerifiedByNavigation)
            .WithMany(p => p.VerifiedProductCertificates)
            .HasForeignKey(d => d.VerifiedBy)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(e => e.ProductId)
            .HasDatabaseName("idx_product");

        builder.HasIndex(e => e.Status)
            .HasDatabaseName("idx_status");

        builder.HasIndex(e => e.UploadedAt)
            .HasDatabaseName("idx_uploaded");
    }
}

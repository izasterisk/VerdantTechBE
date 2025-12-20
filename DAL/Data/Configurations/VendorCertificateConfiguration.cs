using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAL.Data.Models;

namespace DAL.Data.Configurations;

public class VendorCertificateConfiguration : IEntityTypeConfiguration<VendorCertificate>
{
    public void Configure(EntityTypeBuilder<VendorCertificate> builder)
    {
        builder.ToTable("vendor_certificates");

        // Primary Key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("bigint unsigned")
            .ValueGeneratedOnAdd();

        // Foreign Keys
        builder.Property(e => e.VendorId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("vendor_id");

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

        // Enum conversion for status
        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasColumnType("enum('pending','verified','rejected')")
            .HasDefaultValue(VendorCertificateStatus.Pending);

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
        builder.HasOne(d => d.Vendor)
            .WithMany(p => p.VendorCertificates)
            .HasForeignKey(d => d.VendorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.VerifiedByNavigation)
            .WithMany(p => p.VerifiedVendorCertificates)
            .HasForeignKey(d => d.VerifiedBy)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        // Composite index cho queries lấy certificates theo vendor, sort by uploaded_at
        builder.HasIndex(e => new { e.VendorId, e.UploadedAt })
            .HasDatabaseName("idx_vendor_uploaded");
        
        // Composite index cho queries filter theo vendor và status
        builder.HasIndex(e => new { e.VendorId, e.Status })
            .HasDatabaseName("idx_vendor_status");
    }
}

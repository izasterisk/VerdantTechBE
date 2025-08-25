using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAL.Data.Models;
using DAL.Data;

namespace DAL.Data.Configurations;

public class VendorSustainabilityCredentialConfiguration : IEntityTypeConfiguration<VendorSustainabilityCredential>
{
    public void Configure(EntityTypeBuilder<VendorSustainabilityCredential> builder)
    {
        builder.ToTable("vendor_sustainability_credentials");
        
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
            
        builder.Property(e => e.CertificationId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("certification_id");
            
        builder.Property(e => e.VerifiedBy)
            .HasColumnType("bigint unsigned")
            .HasColumnName("verified_by");
        
        // Certificate URL
        builder.Property(e => e.CertificateUrl)
            .HasMaxLength(500)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("certificate_url");
        
        // Status field - enum
        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasDefaultValue(VendorSustainabilityCredentialStatus.Pending)
            .HasColumnName("status");
        
        // Rejection reason
        builder.Property(e => e.RejectionReason)
            .HasColumnType("text")
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("rejection_reason");
        
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
            .WithMany(p => p.VendorSustainabilityCredentials)
            .HasForeignKey(d => d.VendorId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(d => d.Certification)
            .WithMany(p => p.VendorSustainabilityCredentials)
            .HasForeignKey(d => d.CertificationId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(d => d.VerifiedByNavigation)
            .WithMany(p => p.VerifiedSustainabilityCredentials)
            .HasForeignKey(d => d.VerifiedBy)
            .OnDelete(DeleteBehavior.SetNull);
        
        // Unique constraints
        builder.HasIndex(e => new { e.VendorId, e.CertificationId })
            .IsUnique()
            .HasDatabaseName("unique_vendor_certification");
        
        // Indexes
        builder.HasIndex(e => e.VendorId)
            .HasDatabaseName("idx_vendor");
            
        builder.HasIndex(e => e.CertificationId)
            .HasDatabaseName("idx_certification");
            
        builder.HasIndex(e => e.Status)
            .HasDatabaseName("idx_status");
            
        builder.HasIndex(e => e.UploadedAt)
            .HasDatabaseName("idx_uploaded");
            
        builder.HasIndex(e => e.VerifiedAt)
            .HasDatabaseName("idx_verified");
    }
}

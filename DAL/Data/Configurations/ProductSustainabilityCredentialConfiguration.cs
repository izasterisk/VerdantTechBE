using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAL.Data.Models;
using DAL.Data;

namespace DAL.Data.Configurations;

public class ProductSustainabilityCredentialConfiguration : IEntityTypeConfiguration<ProductSustainabilityCredential>
{
    public void Configure(EntityTypeBuilder<ProductSustainabilityCredential> builder)
    {
        builder.ToTable("product_sustainability_credentials");
        
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
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("certificate_url");
        
        // Status field - enum
        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasDefaultValue(ProductSustainabilityCredentialStatus.Pending)
            .HasColumnName("status");
        
        // Rejection reason
        builder.Property(e => e.RejectionReason)
            .HasMaxLength(500)
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
        builder.HasOne(d => d.Product)
            .WithMany(p => p.ProductSustainabilityCredentials)
            .HasForeignKey(d => d.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(d => d.Certification)
            .WithMany(p => p.ProductSustainabilityCredentials)
            .HasForeignKey(d => d.CertificationId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(d => d.VerifiedByUser)
            .WithMany(p => p.VerifiedProductSustainabilityCredentials)
            .HasForeignKey(d => d.VerifiedBy)
            .OnDelete(DeleteBehavior.SetNull);
        
        // Unique constraints
        builder.HasIndex(e => new { e.ProductId, e.CertificationId })
            .IsUnique()
            .HasDatabaseName("unique_product_certification");
        
        // Indexes
        builder.HasIndex(e => e.ProductId)
            .HasDatabaseName("idx_product");
            
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

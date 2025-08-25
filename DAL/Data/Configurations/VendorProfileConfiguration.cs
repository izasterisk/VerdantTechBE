using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using DAL.Data.Models;

namespace DAL.Data.Configurations;

public class VendorProfileConfiguration : IEntityTypeConfiguration<VendorProfile>
{
    public void Configure(EntityTypeBuilder<VendorProfile> builder)
    {
        builder.ToTable("vendor_profiles");
        
        // Primary Key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("bigint unsigned")
            .ValueGeneratedOnAdd();
        
        // Foreign Keys
        builder.Property(e => e.UserId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("user_id");
            
        builder.Property(e => e.VerifiedBy)
            .HasColumnType("bigint unsigned")
            .HasColumnName("verified_by");
        
        // Required fields
        builder.Property(e => e.CompanyName)
            .HasMaxLength(255)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("company_name");
            
        builder.Property(e => e.Slug)
            .HasMaxLength(255)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci");
        
        // Optional unique fields
        builder.Property(e => e.BusinessRegistrationNumber)
            .HasMaxLength(100)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("business_registration_number");
            
        builder.Property(e => e.TaxCode)
            .HasMaxLength(50)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("tax_code");
        
        // Address field
        builder.Property(e => e.CompanyAddress)
            .HasColumnType("text")
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("company_address");
        
        // JSON fields
        builder.Property(e => e.BankAccountInfo)
            .HasConversion(
                v => v == null || v.Count == 0 ? "{}" : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v) || v == "{}" ? new Dictionary<string, object>() : JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("json")
            .HasDefaultValueSql("'{}'")
            .HasColumnName("bank_account_info");
        
        // Decimal fields with precision
        builder.Property(e => e.CommissionRate)
            .HasPrecision(5, 2)
            .HasDefaultValue(10.00m)
            .HasColumnName("commission_rate");
            
        builder.Property(e => e.RatingAverage)
            .HasPrecision(3, 2)
            .HasDefaultValue(0.00m)
            .HasColumnName("rating_average");
        
        // Integer fields with defaults
        builder.Property(e => e.TotalReviews)
            .HasDefaultValue(0)
            .HasColumnName("total_reviews");
        
        // DateTime fields
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
        builder.HasOne(d => d.User)
            .WithOne(p => p.VendorProfile)
            .HasForeignKey<VendorProfile>(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(d => d.VerifiedByNavigation)
            .WithMany(p => p.VerifiedVendorProfiles)
            .HasForeignKey(d => d.VerifiedBy)
            .OnDelete(DeleteBehavior.SetNull);
        
        // Unique constraints
        builder.HasIndex(e => e.UserId)
            .IsUnique()
            .HasDatabaseName("unique_user_id");
            
        builder.HasIndex(e => e.BusinessRegistrationNumber)
            .IsUnique()
            .HasDatabaseName("unique_business_registration");
        
        // Indexes
        builder.HasIndex(e => e.CompanyName)
            .HasDatabaseName("idx_company_name");
            
        builder.HasIndex(e => e.Slug)
            .IsUnique()
            .HasDatabaseName("idx_slug");
            
        builder.HasIndex(e => e.VerifiedAt)
            .HasDatabaseName("idx_verified");
            
        builder.HasIndex(e => e.RatingAverage)
            .HasDatabaseName("idx_rating");
    }
}

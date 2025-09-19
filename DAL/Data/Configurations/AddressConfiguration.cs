using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAL.Data.Models;

namespace DAL.Data.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("addresses");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
            
        builder.Property(e => e.LocationAddress)
            .HasColumnName("location_address")
            .HasColumnType("text");
            
        builder.Property(e => e.Province)
            .HasColumnName("province")
            .HasMaxLength(100);
            
        builder.Property(e => e.District)
            .HasColumnName("district")
            .HasMaxLength(100);
            
        builder.Property(e => e.Commune)
            .HasColumnName("commune")
            .HasMaxLength(100);
            
        builder.Property(e => e.Latitude)
            .HasColumnName("latitude")
            .HasPrecision(10, 8);
            
        builder.Property(e => e.Longitude)
            .HasColumnName("longitude")
            .HasPrecision(11, 8);
            
        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(e => new { e.Province, e.District })
            .HasDatabaseName("idx_province_district");
    }
}

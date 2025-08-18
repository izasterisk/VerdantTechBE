using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAL.Data.Models;
using DAL.Data;

namespace DAL.Data.Configurations;

public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.ToTable("system_settings");
        
        // Primary Key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("bigint unsigned")
            .ValueGeneratedOnAdd();
        
        // Required string field
        builder.Property(e => e.SettingKey)
            .HasMaxLength(100)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("setting_key");
        
        // Optional string fields
        builder.Property(e => e.SettingValue)
            .HasColumnType("text")
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("setting_value");
            
        builder.Property(e => e.Description)
            .HasColumnType("text")
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci");
        
        // Enum conversion for setting type
        builder.Property(e => e.SettingType)
            .HasConversion<string>()
            .HasColumnType("enum('string','number','boolean','json')")
            .HasDefaultValue(SettingType.String)
            .HasColumnName("setting_type");
        
        // Boolean default
        builder.Property(e => e.IsPublic)
            .HasDefaultValue(false)
            .HasColumnName("is_public");
        
        // DateTime fields
        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at");
        
        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
            .HasColumnName("updated_at");
        
        // Unique constraint
        builder.HasIndex(e => e.SettingKey)
            .IsUnique()
            .HasDatabaseName("idx_key");
        
        // Indexes
        builder.HasIndex(e => e.IsPublic)
            .HasDatabaseName("idx_public");
    }
}

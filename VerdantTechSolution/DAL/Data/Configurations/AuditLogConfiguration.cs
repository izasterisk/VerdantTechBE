using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAL.Data.Models;
using System.Text.Json;

namespace DAL.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        // Primary Key
        builder.HasKey(e => e.Id);

        // Table configuration
        builder.ToTable("audit_logs");

        // Property configurations
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(e => e.UserId)
            .HasColumnName("user_id")
            .HasColumnType("bigint unsigned");

        builder.Property(e => e.Action)
            .HasColumnName("action")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.EntityType)
            .HasColumnName("entity_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.EntityId)
            .HasColumnName("entity_id")
            .HasColumnType("bigint unsigned")
            .IsRequired();

        // Configure Dictionary properties as JSON columns
        builder.Property(e => e.OldValues)
            .HasColumnName("old_values")
            .HasColumnType("json")
            .HasConversion(
                v => v == null ? "{}" : JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => string.IsNullOrEmpty(v) ? new Dictionary<string, object>() : JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null!)!
            );

        builder.Property(e => e.NewValues)
            .HasColumnName("new_values")
            .HasColumnType("json")
            .HasConversion(
                v => v == null ? "{}" : JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => string.IsNullOrEmpty(v) ? new Dictionary<string, object>() : JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null!)!
            );

        builder.Property(e => e.IpAddress)
            .HasColumnName("ip_address")
            .HasMaxLength(45);

        builder.Property(e => e.UserAgent)
            .HasColumnName("user_agent")
            .HasColumnType("text");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Foreign Key relationships
        builder.HasOne(e => e.User)
            .WithMany(p => p.AuditLogs)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("idx_audit_logs_user_id");

        builder.HasIndex(e => e.EntityType)
            .HasDatabaseName("idx_audit_logs_entity_type");

        builder.HasIndex(e => e.EntityId)
            .HasDatabaseName("idx_audit_logs_entity_id");

        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("idx_audit_logs_created_at");
    }
}

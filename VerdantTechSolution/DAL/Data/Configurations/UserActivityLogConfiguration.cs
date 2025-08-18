using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using DAL.Data.Models;
using DAL.Data;

namespace DAL.Data.Configurations;

public class UserActivityLogConfiguration : IEntityTypeConfiguration<UserActivityLog>
{
    public void Configure(EntityTypeBuilder<UserActivityLog> builder)
    {
        builder.ToTable("user_activity_logs");
        
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
        
        // Required string field
        builder.Property(e => e.ActivityType)
            .HasMaxLength(50)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("activity_type");
        
        // Optional string fields
        builder.Property(e => e.IpAddress)
            .HasMaxLength(45)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("ip_address");
            
        builder.Property(e => e.UserAgent)
            .HasColumnType("text")
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("user_agent");
            
        builder.Property(e => e.SessionId)
            .HasMaxLength(255)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("session_id");
        
        // JSON field for activity details
        builder.Property(e => e.ActivityDetails)
            .HasConversion(
                v => v == null || v.Count == 0 ? "{}" : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v) || v == "{}" ? new Dictionary<string, object>() : JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("json")
            .HasColumnName("activity_details");
        
        // DateTime field
        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at");
        
        // Foreign Key Relationships
        builder.HasOne(d => d.User)
            .WithMany(p => p.UserActivityLogs)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indexes
        builder.HasIndex(e => new { e.UserId, e.ActivityType })
            .HasDatabaseName("idx_user_type");
            
        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("idx_created");
            
        builder.HasIndex(e => e.SessionId)
            .HasDatabaseName("idx_session");
    }
}

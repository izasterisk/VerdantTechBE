using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAL.Data.Models;

namespace DAL.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");
        
        // Primary Key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("bigint unsigned")
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
        
        // Foreign Keys
        builder.Property(e => e.UserId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("user_id");
            
        // String fields
        builder.Property(e => e.Title)
            .HasMaxLength(255)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("title")
            .HasComment("Tiêu đề thông báo (hiển thị ngắn gọn)");
            
        builder.Property(e => e.Message)
            .HasColumnType("text")
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("message")
            .HasComment("Nội dung chi tiết thông báo");
        
        // Enum conversion for reference type
        builder.Property(e => e.ReferenceType)
            .HasConversion<string>()
            .HasColumnType("enum('order','payment','request','forum_post','chatbot_conversation','refund','wallet_cashout','product_registration','environmental_data')")
            .HasColumnName("reference_type")
            .HasComment("Loại entity tham chiếu (nếu có) - dùng để link đến chi tiết");
            
        builder.Property(e => e.ReferenceId)
            .HasColumnType("bigint unsigned")
            .HasColumnName("reference_id")
            .HasComment("ID của entity tham chiếu (ví dụ: order_id, post_id)");
            
        // Boolean field
        builder.Property(e => e.IsRead)
            .HasColumnType("tinyint(1)")
            .HasDefaultValue(false)
            .HasColumnName("is_read")
            .HasComment("Thông báo đã đọc chưa (dùng để filter hiển thị)");
        
        // Timestamps
        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at");
            
        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAddOrUpdate()
            .HasColumnName("updated_at");
        
        // Indexes
        builder.HasIndex(e => new { e.UserId, e.IsRead })
            .HasDatabaseName("idx_user_read");
            
        builder.HasIndex(e => new { e.ReferenceType, e.ReferenceId })
            .HasDatabaseName("idx_reference");
            
        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("idx_created");
        
        // Relationships
        builder.HasOne(d => d.User)
            .WithMany(p => p.Notifications)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("notifications_ibfk_1");
    }
}

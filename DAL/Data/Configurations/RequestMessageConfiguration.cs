using DAL.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class RequestMessageConfiguration : IEntityTypeConfiguration<RequestMessage>
{
    public void Configure(EntityTypeBuilder<RequestMessage> builder)
    {
        builder.ToTable("request_messages");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasColumnType("bigint unsigned")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.RequestId)
            .HasColumnName("request_id")
            .HasColumnType("bigint unsigned")
            .IsRequired();

        builder.Property(e => e.StaffId)
            .HasColumnName("staff_id")
            .HasColumnType("bigint unsigned");

        builder.Property(e => e.Description)
            .HasColumnName("description")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(e => e.ReplyNotes)
            .HasColumnName("reply_notes")
            .HasColumnType("text");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

        // Foreign keys
        builder.HasOne(e => e.Request)
            .WithMany(r => r.RequestMessages)
            .HasForeignKey(e => e.RequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Staff)
            .WithMany(u => u.RequestMessagesAsStaff)
            .HasForeignKey(e => e.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(e => e.RequestId).HasDatabaseName("idx_request");
    }
}

using DAL.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class RequestConfiguration : IEntityTypeConfiguration<Request>
{
    public void Configure(EntityTypeBuilder<Request> builder)
    {
        builder.ToTable("requests");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasColumnType("bigint unsigned")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.UserId)
            .HasColumnName("user_id")
            .HasColumnType("bigint unsigned");

        builder.Property(e => e.RequestType)
            .HasConversion(
                v => v.ToString()
                    .ToLowerInvariant()
                    .Replace("refundrequest", "refund_request")
                    .Replace("supportrequest", "support_request"),
                v => Enum.Parse<RequestType>(v
                    .Replace("refund_request", "RefundRequest")
                    .Replace("support_request", "SupportRequest"), true))
            .HasColumnName("request_type")
            .HasColumnType("enum('refund_request','support_request')")
            .IsRequired();

        builder.Property(e => e.Title)
            .HasColumnName("title")
            .HasColumnType("varchar(255)")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasColumnName("description")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion(
                v => v.ToString()
                    .ToLowerInvariant()
                    .Replace("inreview", "in_review"),
                v => Enum.Parse<RequestStatus>(v
                    .Replace("in_review", "InReview"), true))
            .HasColumnName("status")
            .HasColumnType("enum('pending','in_review','approved','rejected','completed','cancelled')")
            .HasDefaultValue(RequestStatus.Pending);

        builder.Property(e => e.ReplyNotes)
            .HasColumnName("reply_notes")
            .HasColumnType("text");

        builder.Property(e => e.ProcessedBy)
            .HasColumnName("processed_by")
            .HasColumnType("bigint unsigned");

        builder.Property(e => e.ProcessedAt)
            .HasColumnName("processed_at")
            .HasColumnType("timestamp");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

        // Foreign keys
        builder.HasOne(e => e.User)
            .WithMany(u => u.Requests)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(e => e.ProcessedByNavigation)
            .WithMany(u => u.RequestsProcessed)
            .HasForeignKey(e => e.ProcessedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes (matching schema v7)
        builder.HasIndex(e => new { e.RequestType, e.Status }).HasDatabaseName("idx_type_status");
        builder.HasIndex(e => e.CreatedAt).HasDatabaseName("idx_created_at");
    }
}

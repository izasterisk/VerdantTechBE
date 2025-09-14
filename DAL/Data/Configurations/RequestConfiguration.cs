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

        builder.Property(e => e.RequesterId)
            .HasColumnName("requester_id")
            .HasColumnType("bigint unsigned")
            .IsRequired();

        builder.Property(e => e.RequestType)
            .HasConversion<string>()
            .HasColumnName("request_type")
            .HasColumnType("enum('refund_request','payout_request','support_request')")
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
            .HasConversion<string>()
            .HasColumnName("status")
            .HasColumnType("enum('pending','in_review','approved','rejected','completed','cancelled')")
            .HasDefaultValue(RequestStatus.Pending);

        builder.Property(e => e.Priority)
            .HasConversion<string>()
            .HasColumnName("priority")
            .HasColumnType("enum('low','medium','high','urgent')")
            .HasDefaultValue(RequestPriority.Medium);

        builder.Property(e => e.ReferenceType)
            .HasColumnName("reference_type")
            .HasColumnType("varchar(50)")
            .HasMaxLength(50);

        builder.Property(e => e.ReferenceId)
            .HasColumnName("reference_id")
            .HasColumnType("bigint unsigned");

        builder.Property(e => e.Amount)
            .HasColumnName("amount")
            .HasColumnType("decimal(12,2)");

        builder.Property(e => e.AssignedTo)
            .HasColumnName("assigned_to")
            .HasColumnType("bigint unsigned");
            
        builder.Property(e => e.ProcessedBy)
            .HasColumnName("processed_by")
            .HasColumnType("bigint unsigned");

        builder.Property(e => e.AdminNotes)
            .HasColumnName("admin_notes")
            .HasColumnType("text");

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
        builder.HasOne(e => e.Requester)
            .WithMany(u => u.Requests)
            .HasForeignKey(e => e.RequesterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.AssignedToNavigation)
            .WithMany(u => u.RequestsAssigned)
            .HasForeignKey(e => e.AssignedTo)
            .OnDelete(DeleteBehavior.SetNull);
            
        builder.HasOne(e => e.ProcessedByNavigation)
            .WithMany(u => u.RequestsProcessed)
            .HasForeignKey(e => e.ProcessedBy)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes (matching schema SQL)
        builder.HasIndex(e => e.RequesterId).HasDatabaseName("idx_requester");
        builder.HasIndex(e => new { e.RequestType, e.Status }).HasDatabaseName("idx_type_status");
        builder.HasIndex(e => e.Priority).HasDatabaseName("idx_priority");
        builder.HasIndex(e => e.AssignedTo).HasDatabaseName("idx_assigned");
        builder.HasIndex(e => e.ProcessedBy).HasDatabaseName("idx_processed");
        builder.HasIndex(e => e.CreatedAt).HasDatabaseName("idx_created_at");
        builder.HasIndex(e => new { e.ReferenceType, e.ReferenceId }).HasDatabaseName("idx_reference");
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAL.Data.Models;

namespace DAL.Data.Configurations;

public class SurveyResponseConfiguration : IEntityTypeConfiguration<SurveyResponse>
{
    public void Configure(EntityTypeBuilder<SurveyResponse> builder)
    {
        builder.ToTable("survey_responses");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasColumnType("bigint unsigned");

        builder.Property(e => e.FarmProfileId)
            .HasColumnName("farm_profile_id")
            .HasColumnType("bigint unsigned")
            .HasComment("Trang trại được đánh giá");

        builder.Property(e => e.QuestionId)
            .HasColumnName("question_id")
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasComment("Câu hỏi được trả lời");

        builder.Property(e => e.TextAnswer)
            .HasColumnName("text_answer")
            .HasColumnType("text")
            .HasComment("Câu trả lời dạng text - bắt buộc nếu question_type = text");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
            .ValueGeneratedOnAddOrUpdate();

        // Foreign key relationship
        builder.HasOne(e => e.FarmProfile)
            .WithMany(f => f.SurveyResponses)
            .HasForeignKey(e => e.FarmProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => e.FarmProfileId)
            .HasDatabaseName("idx_farm");
    }
}

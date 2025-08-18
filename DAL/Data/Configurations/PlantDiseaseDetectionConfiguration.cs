using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using DAL.Data.Models;
using DAL.Data;

namespace DAL.Data.Configurations;

public class PlantDiseaseDetectionConfiguration : IEntityTypeConfiguration<PlantDiseaseDetection>
{
    public void Configure(EntityTypeBuilder<PlantDiseaseDetection> builder)
    {
        builder.ToTable("plant_disease_detections");
        
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
            
        builder.Property(e => e.FarmProfileId)
            .HasColumnType("bigint unsigned")
            .HasColumnName("farm_profile_id");
        
        // Required string fields
        builder.Property(e => e.ImageUrl)
            .HasMaxLength(500)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("image_url");
        
        // Optional string fields
        builder.Property(e => e.PlantType)
            .HasMaxLength(100)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("plant_type");
            
        builder.Property(e => e.AiProvider)
            .HasMaxLength(50)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("ai_provider");
            
        builder.Property(e => e.FeedbackComments)
            .HasColumnType("text")
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("feedback_comments");
        
        // JSON fields
        builder.Property(e => e.ImageMetadata)
            .HasConversion(
                v => v == null || v.Count == 0 ? "{}" : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v) || v == "{}" ? new Dictionary<string, object>() : JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("json")
            .HasColumnName("image_metadata");
            
        builder.Property(e => e.DetectedDiseases)
            .HasConversion(
                v => v == null || v.Count == 0 ? "[]" : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v) || v == "[]" ? new List<Dictionary<string, object>>() : JsonSerializer.Deserialize<List<Dictionary<string, object>>>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("json")
            .HasColumnName("detected_diseases");
            
        builder.Property(e => e.AiResponse)
            .HasConversion(
                v => v == null || v.Count == 0 ? "{}" : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v) || v == "{}" ? new Dictionary<string, object>() : JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("json")
            .HasColumnName("ai_response");
            
        builder.Property(e => e.OrganicTreatmentSuggestions)
            .HasConversion(
                v => v == null || v.Count == 0 ? "[]" : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v) || v == "[]" ? new List<string>() : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("json")
            .HasColumnName("organic_treatment_suggestions");
            
        builder.Property(e => e.ChemicalTreatmentSuggestions)
            .HasConversion(
                v => v == null || v.Count == 0 ? "[]" : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v) || v == "[]" ? new List<string>() : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("json")
            .HasColumnName("chemical_treatment_suggestions");
            
        builder.Property(e => e.PreventionTips)
            .HasConversion(
                v => v == null || v.Count == 0 ? "[]" : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v) || v == "[]" ? new List<string>() : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("json")
            .HasColumnName("prevention_tips");
        
        // Enum conversion for user feedback
        builder.Property(e => e.UserFeedback)
            .HasConversion<string>()
            .HasColumnType("enum('helpful','not_helpful','partially_helpful')")
            .HasColumnName("user_feedback");
        
        // DateTime field
        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at");
        
        // Foreign Key Relationships
        builder.HasOne(d => d.User)
            .WithMany(p => p.PlantDiseaseDetections)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(d => d.FarmProfile)
            .WithMany(p => p.PlantDiseaseDetections)
            .HasForeignKey(d => d.FarmProfileId)
            .OnDelete(DeleteBehavior.SetNull);
        
        // Indexes
        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("idx_user");
            
        builder.HasIndex(e => e.FarmProfileId)
            .HasDatabaseName("idx_farm");
            
        builder.HasIndex(e => e.PlantType)
            .HasDatabaseName("idx_plant_type");
            
        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("idx_created");
    }
}

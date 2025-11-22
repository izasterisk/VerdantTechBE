using DAL.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class CropConfiguration : IEntityTypeConfiguration<Crop>
{
    public void Configure(EntityTypeBuilder<Crop> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");

        entity.ToTable("crops", tb => tb.HasComment("Quản lý cây trồng của trang trại (một trang trại có nhiều cây trồng)"));

        entity.HasIndex(e => e.FarmProfileId, "idx_farm_profile");

        entity.HasIndex(e => e.IsActive, "idx_is_active");

        entity.Property(e => e.Id)
            .HasColumnType("bigint(20) unsigned")
            .HasColumnName("id");

        entity.Property(e => e.FarmProfileId)
            .HasColumnType("bigint(20) unsigned")
            .HasColumnName("farm_profile_id");

        entity.Property(e => e.CropName)
            .HasMaxLength(255)
            .HasColumnName("crop_name")
            .HasComment("Tên loại cây trồng");

        entity.Property(e => e.PlantingDate)
            .HasColumnName("planting_date")
            .HasComment("Ngày trồng");

        entity.Property(e => e.IsActive)
            .HasDefaultValue(true)
            .HasColumnName("is_active")
            .HasComment("Trạng thái hoạt động");

        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("current_timestamp()")
            .HasColumnType("timestamp")
            .HasColumnName("created_at");

        entity.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("current_timestamp()")
            .ValueGeneratedOnAddOrUpdate()
            .HasColumnType("timestamp")
            .HasColumnName("updated_at");

        entity.HasOne(d => d.FarmProfile)
            .WithMany(p => p.Crops)
            .HasForeignKey(d => d.FarmProfileId)
            .HasConstraintName("crops_ibfk_1");
    }
}

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

        entity.HasIndex(e => e.Status, "idx_status");

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

        entity.Property(e => e.PlantingMethod)
            .IsRequired()
            .HasConversion(
                v => v.ToString().ToLowerInvariant()
                    .Replace("directseeding", "direct_seeding")
                    .Replace("traynursery", "tray_nursery")
                    .Replace("vegetativepropagation", "vegetative_propagation"),
                v => Enum.Parse<PlantingMethod>(v
                    .Replace("direct_seeding", "DirectSeeding")
                    .Replace("tray_nursery", "TrayNursery")
                    .Replace("vegetative_propagation", "VegetativePropagation"), true))
            .HasColumnType("enum('direct_seeding','tray_nursery','transplanting','vegetative_propagation','cutting')")
            .HasColumnName("planting_method")
            .HasComment("Phương pháp gieo trồng: direct_seeding (Gieo hạt trực tiếp), tray_nursery (Ươm trong khay), transplanting (Cấy cây con), vegetative_propagation (Sinh sản sinh dưỡng từ củ/thân), cutting (Giâm cành)");

        entity.Property(e => e.CropType)
            .IsRequired()
            .HasConversion(
                v => v.ToString().ToLowerInvariant()
                    .Replace("leafygreen", "leafy_green")
                    .Replace("rootvegetable", "root_vegetable"),
                v => Enum.Parse<CropType>(v
                    .Replace("leafy_green", "LeafyGreen")
                    .Replace("root_vegetable", "RootVegetable"), true))
            .HasColumnType("enum('leafy_green','fruiting','root_vegetable','herb')")
            .HasColumnName("crop_type")
            .HasComment("Loại cây trồng: leafy_green (Rau ăn lá như rau muống, cải, xà lách), fruiting (Rau ăn quả như cà chua, dưa leo, ớt), root_vegetable (Củ như cà rốt, củ cải), herb (Rau thơm như húng, ngò)");

        entity.Property(e => e.FarmingType)
            .IsRequired()
            .HasConversion(
                v => v.ToString().ToLowerInvariant()
                    .Replace("croprotation", "crop_rotation"),
                v => Enum.Parse<FarmingType>(v
                    .Replace("crop_rotation", "CropRotation"), true))
            .HasColumnType("enum('intensive','crop_rotation','intercropping','greenhouse','hydroponics')")
            .HasColumnName("farming_type")
            .HasComment("Loại hình canh tác: intensive (Thâm canh), crop_rotation (Luân canh), intercropping (Xen canh), greenhouse (Nhà lưới/nhà màng), hydroponics (Thủy canh)");

        entity.Property(e => e.Status)
            .HasConversion(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<CropStatus>(v, true))
            .HasColumnType("enum('planning','seedling','growing','harvesting','completed','failed','deleted')")
            .HasDefaultValue(CropStatus.Planning)
            .HasColumnName("status")
            .HasComment("Trạng thái của cây trồng: planning (Đang lên kế hoạch), seedling (Giai đoạn cây con), growing (Đang sinh trưởng), harvesting (Đang thu hoạch), completed (Hoàn thành), failed (Thất bại), deleted (Đã xóa)");

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

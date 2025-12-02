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
                    .Replace("gieohattructiep", "gieo_hat_truc_tiep")
                    .Replace("uomtrongkhay", "uom_trong_khay")
                    .Replace("caycaycon", "cay_cay_con")
                    .Replace("sinhsansinhduong", "sinh_san_sinh_duong")
                    .Replace("giamcanh", "giam_canh"),
                v => Enum.Parse<PlantingMethod>(v
                    .Replace("gieo_hat_truc_tiep", "GieoHatTrucTiep")
                    .Replace("uom_trong_khay", "UomTrongKhay")
                    .Replace("cay_cay_con", "CayCayCon")
                    .Replace("sinh_san_sinh_duong", "SinhSanSinhDuong")
                    .Replace("giam_canh", "GiamCanh"), true))
            .HasColumnType("enum('gieo_hat_truc_tiep','uom_trong_khay','cay_cay_con','sinh_san_sinh_duong','giam_canh')")
            .HasColumnName("planting_method")
            .HasComment("Phương pháp gieo trồng: gieo_hat_truc_tiep (Gieo hạt trực tiếp), uom_trong_khay (Ươm trong khay), cay_cay_con (Cấy cây con), sinh_san_sinh_duong (Sinh sản sinh dưỡng từ củ/thân), giam_canh (Giâm cành)");

        entity.Property(e => e.CropType)
            .IsRequired()
            .HasConversion(
                v => v.ToString().ToLowerInvariant()
                    .Replace("rauanla", "rau_an_la")
                    .Replace("rauanqua", "rau_an_qua")
                    .Replace("raucu", "rau_cu")
                    .Replace("rauthom", "rau_thom"),
                v => Enum.Parse<CropType>(v
                    .Replace("rau_an_la", "RauAnLa")
                    .Replace("rau_an_qua", "RauAnQua")
                    .Replace("rau_cu", "RauCu")
                    .Replace("rau_thom", "RauThom"), true))
            .HasColumnType("enum('rau_an_la','rau_an_qua','rau_cu','rau_thom')")
            .HasColumnName("crop_type")
            .HasComment("Loại cây trồng: rau_an_la (Rau ăn lá như rau muống, cải, xà lách), rau_an_qua (Rau ăn quả như cà chua, dưa leo, ớt), rau_cu (Củ như cà rốt, củ cải), rau_thom (Rau thơm như húng, ngò)");

        entity.Property(e => e.FarmingType)
            .IsRequired()
            .HasConversion(
                v => v.ToString().ToLowerInvariant()
                    .Replace("thamcanh", "tham_canh")
                    .Replace("luancanh", "luan_canh")
                    .Replace("xencanh", "xen_canh")
                    .Replace("haluoi", "nha_luoi")
                    .Replace("thuycanh", "thuy_canh"),
                v => Enum.Parse<FarmingType>(v
                    .Replace("tham_canh", "ThamCanh")
                    .Replace("luan_canh", "LuanCanh")
                    .Replace("xen_canh", "XenCanh")
                    .Replace("nha_luoi", "NhaLuoi")
                    .Replace("thuy_canh", "ThuyCanh"), true))
            .HasColumnType("enum('tham_canh','luan_canh','xen_canh','nha_luoi','thuy_canh')")
            .HasColumnName("farming_type")
            .HasComment("Loại hình canh tác: tham_canh (Thâm canh), luan_canh (Luân canh), xen_canh (Xen canh), nha_luoi (Nhà lưới/nhà màng), thuy_canh (Thủy canh)");

        entity.Property(e => e.Status)
            .HasConversion(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<CropStatus>(v, true))
            .HasColumnType("enum('growing','harvested','failed','deleted')")
            .HasDefaultValue(CropStatus.Growing)
            .HasColumnName("status")
            .HasComment("Trạng thái của cây trồng: growing (Đang sinh trưởng), harvested (Đã thu hoạch), failed (Thất bại), deleted (Đã xóa)");

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

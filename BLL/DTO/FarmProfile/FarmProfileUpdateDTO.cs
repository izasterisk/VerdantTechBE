using System.ComponentModel.DataAnnotations;
using DAL.Data;

namespace BLL.DTO.FarmProfile;

public class FarmProfileUpdateDTO
{
    [StringLength(255, ErrorMessage = "Tên trang trại không được vượt quá 255 ký tự")]
    public string? FarmName { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Diện tích trang trại phải lớn hơn 0")]
    public decimal? FarmSizeHectares { get; set; }

    [MaxLength(500, ErrorMessage = "Địa chỉ cụ thể không được vượt quá 500 ký tự")]
    public string? LocationAddress { get; set; }

    [StringLength(100, ErrorMessage = "Tỉnh/Thành phố không được vượt quá 100 ký tự")]
    public string? Province { get; set; }

    [StringLength(100, ErrorMessage = "Quận/Huyện không được vượt quá 100 ký tự")]
    public string? District { get; set; }

    [StringLength(100, ErrorMessage = "Xã/Phường không được vượt quá 100 ký tự")]
    public string? Commune { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "Mã tỉnh/thành phải lớn hơn 0")]
    public int? ProvinceCode { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Mã quận/huyện phải lớn hơn 0")]
    public int? DistrictCode { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Mã xã/phường phải lớn hơn 0")]
    public int? CommuneCode { get; set; }

    [Range(-90, 90, ErrorMessage = "Vĩ độ phải nằm trong khoảng -90 đến 90")]
    public decimal? Latitude { get; set; }

    [Range(-180, 180, ErrorMessage = "Kinh độ phải nằm trong khoảng -180 đến 180")]
    public decimal? Longitude { get; set; }
    
    [EnumDataType(typeof(FarmProfileStatus), ErrorMessage = "Trạng thái phải là Active, Maintenance hoặc Deleted")]
    public FarmProfileStatus? Status { get; set; }

    [StringLength(500, ErrorMessage = "Thông tin cây trồng chính không được vượt quá 500 ký tự")]
    public string? PrimaryCrops { get; set; }
}
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
    
    [StringLength(20, ErrorMessage = "Mã tỉnh/thành không được vượt quá 20 ký tự")]
    public string? ProvinceCode { get; set; }

    [StringLength(20, ErrorMessage = "Mã quận/huyện không được vượt quá 20 ký tự")]
    public string? DistrictCode { get; set; }

    [StringLength(20, ErrorMessage = "Mã xã/phường không được vượt quá 20 ký tự")]
    public string? CommuneCode { get; set; }

    [Range(-90, 90, ErrorMessage = "Vĩ độ phải nằm trong khoảng -90 đến 90")]
    public decimal? Latitude { get; set; }

    [Range(-180, 180, ErrorMessage = "Kinh độ phải nằm trong khoảng -180 đến 180")]
    public decimal? Longitude { get; set; }
    
    [EnumDataType(typeof(FarmProfileStatus), ErrorMessage = "Trạng thái phải là Active, Maintenance hoặc Deleted")]
    public FarmProfileStatus? Status { get; set; }

    public List<CropsUpdateDTO>? CropsUpdate { get; set; }
    public List<CropsCreateDTO>? CropsCreate { get; set; }
}
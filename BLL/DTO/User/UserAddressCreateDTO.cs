using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.User;

public class UserAddressCreateDTO
{
    [Required(ErrorMessage = "Địa chỉ cụ thể là bắt buộc")]
    [MaxLength(500, ErrorMessage = "Địa chỉ cụ thể không được vượt quá 500 ký tự")]
    public string LocationAddress { get; set; } = null!;

    [Required(ErrorMessage = "Tên tỉnh/thành phố là bắt buộc")]
    [StringLength(100, ErrorMessage = "Tên tỉnh/thành phố không được vượt quá 100 ký tự")]
    public string Province { get; set; } = null!;

    [Required(ErrorMessage = "Tên quận/huyện là bắt buộc")]
    [StringLength(100, ErrorMessage = "Tên quận/huyện không được vượt quá 100 ký tự")]
    public string District { get; set; } = null!;

    [Required(ErrorMessage = "Tên xã/phường là bắt buộc")]
    [StringLength(100, ErrorMessage = "Tên xã/phường không được vượt quá 100 ký tự")]
    public string Commune { get; set; } = null!;
    
    [Required(ErrorMessage = "Mã tỉnh/thành là bắt buộc")]
    [StringLength(100, ErrorMessage = "Mã tỉnh/thành không được vượt quá 100 ký tự")]
    public string ProvinceCode { get; set; } = null!;

    [Required(ErrorMessage = "Mã quận/huyện là bắt buộc")]
    [StringLength(100, ErrorMessage = "Mã quận/huyện không được vượt quá 100 ký tự")]
    public string DistrictCode { get; set; } = null!;

    [Required(ErrorMessage = "Mã xã/phường là bắt buộc")]
    [StringLength(100, ErrorMessage = "Mã xã/phường không được vượt quá 100 ký tự")]
    public string CommuneCode { get; set; } = null!;

    [Required(ErrorMessage = "Vĩ độ là bắt buộc")]
    [Range(-90.0, 90.0, ErrorMessage = "Vĩ độ phải nằm trong khoảng từ -90 đến 90")]
    public decimal Latitude { get; set; }

    [Required(ErrorMessage = "Kinh độ là bắt buộc")]
    [Range(-180.0, 180.0, ErrorMessage = "Kinh độ phải nằm trong khoảng từ -180 đến 180")]
    public decimal Longitude { get; set; }
}
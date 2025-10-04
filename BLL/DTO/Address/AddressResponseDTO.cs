using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.Address;

public class AddressResponseDTO
{
    public ulong Id { get; set; }

    [MaxLength(500, ErrorMessage = "Địa chỉ cụ thể không được vượt quá 500 ký tự")]
    public string? LocationAddress { get; set; }

    [StringLength(100, ErrorMessage = "Tên tỉnh/thành phố không được vượt quá 100 ký tự")]
    public string? Province { get; set; }

    [StringLength(100, ErrorMessage = "Tên quận/huyện không được vượt quá 100 ký tự")]
    public string? District { get; set; }

    [StringLength(100, ErrorMessage = "Tên xã/phường không được vượt quá 100 ký tự")]
    public string? Commune { get; set; }
    
    [StringLength(100, ErrorMessage = "Mã tỉnh/thành không được vượt quá 100 ký tự")]
    public string? ProvinceCode { get; set; }

    [StringLength(100, ErrorMessage = "Mã quận/huyện không được vượt quá 100 ký tự")]
    public string? DistrictCode { get; set; }

    [StringLength(100, ErrorMessage = "Mã xã/phường không được vượt quá 100 ký tự")]
    public string? CommuneCode { get; set; }

    [Range(-90.0, 90.0, ErrorMessage = "Vĩ độ phải nằm trong khoảng từ -90 đến 90")]
    public decimal? Latitude { get; set; }

    [Range(-180.0, 180.0, ErrorMessage = "Kinh độ phải nằm trong khoảng từ -180 đến 180")]
    public decimal? Longitude { get; set; }

    public DateTime? CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
}
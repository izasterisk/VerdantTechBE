using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.FarmProfile;
    public class FarmProfileCreateDto
    {
        [Required(ErrorMessage = "Tên trang trại là bắt buộc")]
        [StringLength(255, ErrorMessage = "Tên trang trại không được vượt quá 255 ký tự")]
        public string FarmName { get; set; } = null!;

        [Required(ErrorMessage = "Diện tích trang trại không được để trống")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Diện tích trang trại phải lớn hơn 0")]
        public decimal FarmSizeHectares { get; set; }

        [Required(ErrorMessage = "Địa chỉ cụ thể không được để trống")]
        [MaxLength(500, ErrorMessage = "Địa chỉ cụ thể không được vượt quá 500 ký tự")]
        public string LocationAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên tỉnh/thành phố không được để trống")]
        [StringLength(100, ErrorMessage = "Tên tỉnh/thành phố không được vượt quá 100 ký tự")]
        public string Province { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên quận/huyện không được để trống")]
        [StringLength(100, ErrorMessage = "Tên quận/huyện không được vượt quá 100 ký tự")]
        public string District { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên xã/phường không được để trống")]
        [StringLength(100, ErrorMessage = "Tên xã/phường không được vượt quá 100 ký tự")]
        public string Commune { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Mã tỉnh/thành là bắt buộc")]
        [StringLength(20, ErrorMessage = "Mã tỉnh/thành không được vượt quá 20 ký tự")]
        public string ProvinceCode { get; set; } = null!;

        [Required(ErrorMessage = "Mã quận/huyện là bắt buộc")]
        [StringLength(20, ErrorMessage = "Mã quận/huyện không được vượt quá 20 ký tự")]
        public string DistrictCode { get; set; } = null!;

        [Required(ErrorMessage = "Mã xã/phường là bắt buộc")]
        [StringLength(20, ErrorMessage = "Mã xã/phường không được vượt quá 20 ký tự")]
        public string CommuneCode { get; set; } = null!;        
        
        [Required(ErrorMessage = "Vĩ độ không được để trống")]
        [Range(-90, 90, ErrorMessage = "Vĩ độ phải nằm trong khoảng -90 đến 90")]
        public decimal Latitude { get; set; }

        [Required(ErrorMessage = "Kinh độ không được để trống")]
        [Range(-180, 180, ErrorMessage = "Kinh độ phải nằm trong khoảng -180 đến 180")]
        public decimal Longitude { get; set; }

        public List<CropsCreateDTO>? Crops { get; set; }
    }

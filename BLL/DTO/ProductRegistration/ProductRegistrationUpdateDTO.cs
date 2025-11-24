using System.ComponentModel.DataAnnotations;
using static BLL.DTO.Product.ProductUpdateDTO;

namespace BLL.DTO.ProductRegistration
{
    public class ProductRegistrationUpdateDTO
    {
        // ID vẫn bắt buộc để biết đang sửa bản ghi nào
        [Required(ErrorMessage = "ProductRegistrationId là bắt buộc")]
        public ulong Id { get; set; }

        // Các trường khóa ngoại chuyển sang nullable
        public ulong? VendorId { get; set; }
        public ulong? CategoryId { get; set; }

        [StringLength(100, ErrorMessage = "Mã sản phẩm đề xuất không được vượt quá 100 ký tự")]
        public string? ProposedProductCode { get; set; }

        [StringLength(255, ErrorMessage = "Tên sản phẩm đề xuất không được vượt quá 255 ký tự")]
        public string? ProposedProductName { get; set; }

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }

        // decimal -> decimal? để biết có gửi giá hay không
        public decimal? UnitPrice { get; set; }

        [StringLength(10, ErrorMessage = "Xếp hạng hiệu suất năng lượng...")]
        public string? EnergyEfficiencyRating { get; set; }

        public Dictionary<string, object>? Specifications { get; set; }

        // int -> int? (mặc định vẫn có thể để logic gán 12 ở tầng Service nếu tạo mới, nhưng update thì nên để null)
        public int? WarrantyMonths { get; set; }

        [Range(0.001, 50000, ErrorMessage = "Khối lượng sản phẩm phải từ 0.001 đến 50.000 kg")]
        // decimal -> decimal?
        public decimal? WeightKg { get; set; }

        // Bỏ từ khóa 'required', cho phép null
        public DimensionsDTO? DimensionsCm { get; set; }

        // List nên để null thay vì new(), nếu null tức là không update danh sách này
        public List<string>? CertificationCode { get; set; }
        public List<string>? CertificationName { get; set; }
    }
}
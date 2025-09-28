using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO.Product
{
    public class ProductUpdateDTO
    {

        [Required(ErrorMessage = "Danh mục sản phẩm là bắt buộc")]
        public ulong CategoryId { get; set; }

        [Required(ErrorMessage = "Nhà cung cấp là bắt buộc")]
        public ulong VendorId { get; set; }

        [Required(ErrorMessage = "Mã sản phẩm là bắt buộc")]
        [StringLength(100, ErrorMessage = "Mã sản phẩm không được vượt quá 100 ký tự")]
        public string ProductCode { get; set; } = null!;

        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        [StringLength(255, ErrorMessage = "Tên sản phẩm không được vượt quá 255 ký tự")]
        public string ProductName { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Giá đơn vị là bắt buộc")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá đơn vị phải lớn hơn 0")]
        public decimal UnitPrice { get; set; }

        [Range(0.00, 100.00, ErrorMessage = "Tỷ lệ hoa hồng phải nằm trong khoảng 0 đến 100")]
        public decimal CommissionRate { get; set; } = 0.00m;

        [Range(0.00, 100.00, ErrorMessage = "Phần trăm giảm giá phải nằm trong khoảng 0 đến 100")]
        public decimal DiscountPercentage { get; set; } = 0.00m;

        [StringLength(10, ErrorMessage = "Xếp hạng hiệu suất năng lượng không được vượt quá 10 ký tự")]
        public string? EnergyEfficiencyRating { get; set; }

        /// <summary>
        /// Technical specifications as key-value pairs (JSON)
        /// </summary>
        public Dictionary<string, object> Specifications { get; set; } = new();

        /// <summary>
        /// Manual/guide URLs, comma-separated
        /// </summary>
        [StringLength(1000, ErrorMessage = "Các URL hướng dẫn không được vượt quá 1000 ký tự")]
        public string? ManualUrls { get; set; }

        /// <summary>
        /// Image URLs, comma-separated
        /// </summary>
        [StringLength(1000, ErrorMessage = "Các URL hình ảnh không được vượt quá 1000 ký tự")]
        public string? Images { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Thời gian bảo hành phải là số dương")]
        public int WarrantyMonths { get; set; } = 12;

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng hàng tồn kho phải lớn hơn hoặc bằng 0")]
        public int StockQuantity { get; set; } = 0;

        public decimal? WeightKg { get; set; }

        /// <summary>
        /// {length, width, height} (JSON)
        /// </summary>
        public Dictionary<string, decimal> DimensionsCm { get; set; } = new();

        public bool IsActive { get; set; } = true;

        public long ViewCount { get; set; } = 0L;

        public long SoldCount { get; set; } = 0L;

        [Range(0.00, 5.00, ErrorMessage = "Đánh giá trung bình phải nằm trong khoảng 0 đến 5")]
        public decimal RatingAverage { get; set; } = 0.00m;

    }
}

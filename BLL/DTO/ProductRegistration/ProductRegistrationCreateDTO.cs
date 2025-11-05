using BLL.DTO.MediaLink;
using DAL.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BLL.DTO.Product.ProductUpdateDTO;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BLL.DTO.ProductRegistration
{
    public class ProductRegistrationCreateDTO
    {
       //[Required] public ulong Id { get; set; }

        [Required] public ulong VendorId { get; set; }

        [Required(ErrorMessage = "Danh mục sản phẩm là bắt buộc")]
        public ulong CategoryId { get; set; }

        [Required(ErrorMessage = "Mã sản phẩm đề xuất là bắt buộc")]
        [StringLength(100, ErrorMessage = "Mã sản phẩm đề xuất không được vượt quá 100 ký tự")]
        public string ProposedProductCode { get; set; } = null!;

        [Required(ErrorMessage = "Tên sản phẩm đề xuất là bắt buộc")]
        [StringLength(255, ErrorMessage = "Tên sản phẩm đề xuất không được vượt quá 255 ký tự")]
        public string ProposedProductName { get; set; } = null!;

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]

        public string? Description { get; set; }

        [Required(ErrorMessage = "Giá sản phẩm là bắt buộc")]
        public decimal UnitPrice { get; set; }

        [StringLength(10, ErrorMessage = "Xếp hạng hiệu suất năng lượng không được vượt quá 10, 1 cấp độ tương đương 0,5 sao")]
        public string? EnergyEfficiencyRating { get; set; }

        //[Required(ErrorMessage = "Thông số kỹ thuật là bắt buộc")]
        /// <summary>Điền JSON text ở form field tên "specifications".</summary>
        //[FromForm(Name = "specifications")]
        public Dictionary<string, object>? Specifications { get; set; }

        //[StringLength(1000, ErrorMessage = "Liên kết hướng dẫn sử dụng không được vượt quá 1000 ký tự")]
        //public string? ManualUrls { get; set; }

        //[StringLength(1000, ErrorMessage = "Danh sách hình ảnh không được vượt quá 1000 ký tự")]
        //public string? Images { get; set; }

        //[Required(ErrorMessage = "Thời gian bảo hành là bắt buộc")]
        public int WarrantyMonths { get; set; } = 12;

        [Required(ErrorMessage = "Trọng lượng sản phẩm là bắt buộc")]
        [Range(0.001, 50000, ErrorMessage = "Khối lượng sản phẩm phải từ 0.001 đến 50.000 kg")]
        public decimal WeightKg { get; set; }

        [Required(ErrorMessage = "Kích thước sản phẩm là bắt buộc")]
        public required DimensionsDTO DimensionsCm { get; set; }

        [Required, StringLength(50)]
        public string CertificationCode { get; set; } = null!;

        [Required, StringLength(255)]
        public string CertificationName { get; set; } = null!;

        //// Manual lưu ở field riêng, controller set sau khi upload
        //[JsonIgnore] 
        ////[BindNever] [SwaggerSchema(ReadOnly = true)]
        //public string? ManualUrl { get; set; }
        //[JsonIgnore] 
        ////[BindNever] [SwaggerSchema(ReadOnly = true)]
        //public string? ManualPublicUrl { get; set; }

        //// Ảnh & chứng chỉ: lưu ở MediaLinks
        //[JsonIgnore] 
        ////[BindNever] [SwaggerSchema(ReadOnly = true)]
        //public List<MediaLinkItemDTO>? ProductImages { get; set; }
        //[JsonIgnore] 
        ////[BindNever] [SwaggerSchema(ReadOnly = true)]
        //public List<MediaLinkItemDTO>? CertificateFiles { get; set; }


    }
}

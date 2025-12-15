using DAL.Data;
using System.ComponentModel.DataAnnotations;
using BLL.Helpers.ProductUpdateRequest;
using Microsoft.AspNetCore.Http;

namespace BLL.DTO.ProductUpdateRequest;

public class ProductUpdateRequestCreateDTO
{
    // public ulong Id { get; set; }
    
    [Required(ErrorMessage = "ProductId là bắt buộc")]
    public ulong ProductId { get; set; }

    // public ulong? CategoryId { get; set; }

    // public ulong VendorId { get; set; }

    [StringLength(100, ErrorMessage = "Mã sản phẩm không được vượt quá 100 ký tự")]
    public string? ProductCode { get; set; }

    [StringLength(255, ErrorMessage = "Tên sản phẩm không được vượt quá 255 ký tự")]
    public string? ProductName { get; set; }

    // public string Slug { get; set; } = null!;

    [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
    public string? Description { get; set; }

    [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Giá đơn vị phải lớn hơn 0")]
    public decimal? UnitPrice { get; set; }

    // public decimal CommissionRate { get; set; } = 0.00m;

    [Range(0.00, 100.00, ErrorMessage = "Phần trăm giảm giá phải nằm trong khoảng 0 đến 100")]
    public decimal? DiscountPercentage { get; set; }

    [Range(0, 5, ErrorMessage = "Xếp hạng hiệu suất năng lượng phải từ 0 đến 5")]
    public int? EnergyEfficiencyRating { get; set; }

    public Dictionary<string, object>? Specifications { get; set; }

    // public string? ManualUrls { get; set; }

    // public string? PublicUrl { get; set; }
    public IFormFile? ManualFile { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Thời gian bảo hành phải lớn hơn hoặc bằng 0")]
    public int? WarrantyMonths { get; set; }

    [Range(0.001, 50000, ErrorMessage = "Khối lượng sản phẩm phải từ 0.001 đến 50.000 kg")]
    public decimal? WeightKg { get; set; }

    public Dictionary<string, decimal>? DimensionsCm { get; set; }

    // public ProductRegistrationStatus Status { get; set; } = ProductRegistrationStatus.Pending;

    // public string? RejectionReason { get; set; }

    // public ulong? ProcessedBy { get; set; }

    // public DateTime? ProcessedAt { get; set; }

    // public DateTime CreatedAt { get; set; }

    // public DateTime UpdatedAt { get; set; }
    
    public List<IFormFile>? Images { get; set; }
}
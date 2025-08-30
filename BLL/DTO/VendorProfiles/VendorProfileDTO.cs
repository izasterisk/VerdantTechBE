using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.VendorProfiles;

public class VendorProfileDTO
{
    public ulong Id { get; set; }

    [Required(ErrorMessage = "Yêu cầu UserId")]
    public ulong UserId { get; set; }

    [Required(ErrorMessage = "Yêu cầu tên công ty")]
    [StringLength(255, ErrorMessage = "Tên công ty không được vượt quá 255 ký tự")]
    public string CompanyName { get; set; } = null!;

    [Required(ErrorMessage = "Yêu cầu Slug")]
    [StringLength(255, ErrorMessage = "Slug không được vượt quá 255 ký tự")]
    [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Slug phải là chữ thường, chữ số và dấu gạch ngang")]
    public string Slug { get; set; } = null!;

    [StringLength(100, ErrorMessage = "Số đăng ký kinh doanh không được vượt quá 100 ký tự")]
    public string? BusinessRegistrationNumber { get; set; }

    [StringLength(50, ErrorMessage = "Mã số thuế không được vượt quá 50 ký tự")]
    public string? TaxCode { get; set; }

    public string? CompanyAddress { get; set; }

    public List<string> SustainabilityCredentials { get; set; } = new();

    public DateTime? VerifiedAt { get; set; }

    public ulong? VerifiedBy { get; set; }

    public Dictionary<string, object> BankAccountInfo { get; set; } = new();

    [Range(0, 100, ErrorMessage = "Tỷ lệ hoa hồng phải nằm trong khoảng từ 0 đến 100")]
    public decimal CommissionRate { get; set; } = 10.00m;

    [Range(0, 5, ErrorMessage = "Đánh giá trung bình phải nằm trong khoảng từ 0 đến 5")]
    public decimal RatingAverage { get; set; } = 0.00m;

    [Range(0, int.MaxValue, ErrorMessage = "Tổng số đánh giá phải không âm")]
    public int TotalReviews { get; set; } = 0;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
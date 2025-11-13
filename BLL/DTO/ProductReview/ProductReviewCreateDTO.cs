using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.ProductReview;

public class ProductReviewCreateDTO
{
    [Required(ErrorMessage = "ProductId là bắt buộc")]
    public ulong ProductId { get; set; }

    [Required(ErrorMessage = "OrderId là bắt buộc")]
    public ulong OrderId { get; set; }

    [Required(ErrorMessage = "Rating là bắt buộc")]
    [Range(1, 5, ErrorMessage = "Rating phải từ 1 đến 5")]
    public int Rating { get; set; }

    [StringLength(2000, ErrorMessage = "Comment không được vượt quá 2000 ký tự")]
    public string? Comment { get; set; }
}

public class ProductReviewImageDTO
{
    [Required(ErrorMessage = "ImageUrl là bắt buộc")]
    public string ImageUrl { get; set; } = null!;
    
    [Required(ErrorMessage = "ImagePublicId là bắt buộc")]
    public string ImagePublicId { get; set; } = null!;
}


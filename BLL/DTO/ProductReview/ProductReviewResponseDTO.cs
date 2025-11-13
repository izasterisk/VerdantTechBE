using BLL.DTO.User;

namespace BLL.DTO.ProductReview;

public class ProductReviewResponseDTO
{
    public ulong Id { get; set; }
    public ulong ProductId { get; set; }
    public ulong OrderId { get; set; }
    public ulong CustomerId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation Properties
    public UserResponseDTO? Customer { get; set; }
    
    // Images
    public List<ProductReviewImageDTO>? Images { get; set; }
}


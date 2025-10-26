using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.Payment.PayOS;
public class CreatePaymentDataDTO
{
    // [Required(ErrorMessage = "Order code is required")]
    // [Range(1, long.MaxValue, ErrorMessage = "Order code must be greater than 0")]
    // public long OrderCode { get; set; }

    // [Required(ErrorMessage = "Amount is required")]
    // [Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    // public int Amount { get; set; }

    [Required(ErrorMessage = "Description is required")]
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string Description { get; set; } = null!;

    [Required(ErrorMessage = "Items are required")]
    [MinLength(1, ErrorMessage = "At least one item is required")]
    public List<PaymentItemDTO> Items { get; set; } = new();

    [Required(ErrorMessage = "Cancel URL is required")]
    [Url(ErrorMessage = "Cancel URL must be a valid URL")]
    public string CancelUrl { get; set; } = null!;

    [Required(ErrorMessage = "Return URL is required")]
    [Url(ErrorMessage = "Return URL must be a valid URL")]
    public string ReturnUrl { get; set; } = null!;

    // Optional fields
    public string? Signature { get; set; }

    [StringLength(100, ErrorMessage = "Buyer name cannot exceed 100 characters")]
    public string? BuyerName { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string? BuyerEmail { get; set; }

    [Phone(ErrorMessage = "Invalid phone number")]
    [RegularExpression(@"^(0|\+84)[3|5|7|8|9][0-9]{8}$", ErrorMessage = "Phone number must be a valid Vietnamese phone number")]
    public string? BuyerPhone { get; set; }

    [StringLength(500, ErrorMessage = "Buyer address cannot exceed 500 characters")]
    public string? BuyerAddress { get; set; }

    [Range(0, long.MaxValue, ErrorMessage = "Expired at must be a valid timestamp")]
    public long? ExpiredAt { get; set; }
}
public class PaymentItemDTO
{
    [Required(ErrorMessage = "Item name is required")]
    [StringLength(200, ErrorMessage = "Item name cannot exceed 200 characters")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Quantity is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "Price is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public int Price { get; set; }
}
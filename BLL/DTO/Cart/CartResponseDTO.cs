using BLL.DTO.User;

namespace BLL.DTO.Cart;

public class CartResponseDTO
{
    public UserResponseDTO UserInfoDTO { get; set; } = null!;
    public List<CartItemDTO> CartItems { get; set; } = new();
}

public class CartItemDTO
{
    public ulong ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Description { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public string? Images { get; set; }
    public bool IsActive { get; set; } = true;
    public long SoldCount { get; set; } = 0L;
    public decimal RatingAverage { get; set; } = 0.00m;
}
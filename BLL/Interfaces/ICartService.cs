using BLL.DTO.Cart;

namespace BLL.Interfaces;

public interface ICartService
{
    Task<CartResponseDTO> AddToCartAsync(ulong userId, CartDTO dto, CancellationToken cancellationToken = default);
    Task<CartResponseDTO> UpdateCartItemQuantityAsync(ulong userId, CartDTO dto, CancellationToken cancellationToken = default);
    Task<CartResponseDTO?> GetCartByUserIdAsync(ulong userId, CancellationToken cancellationToken = default);
}
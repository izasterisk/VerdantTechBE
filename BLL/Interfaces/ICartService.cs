using BLL.DTO.Cart;

namespace BLL.Interfaces;

public interface ICartService
{
    Task<CartResponseDTO> AddToCartAsync(ulong userId, CartAddDTO dto, CancellationToken cancellationToken = default);
    Task<CartResponseDTO> UpdateCartItemAsync(ulong userId, CartAddDTO dto, CancellationToken cancellationToken = default);
    Task<CartResponseDTO?> GetCartByUserIdAsync(ulong userId, CancellationToken cancellationToken = default);
}
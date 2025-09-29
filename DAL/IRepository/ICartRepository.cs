using DAL.Data.Models;

namespace DAL.IRepository;

public interface ICartRepository
{
    Task<Cart> CreateCartByUserIdWithTransactionAsync(ulong userId, CancellationToken cancellationToken = default);
    Task<Cart?> GetCartByUserIdWithRelationsAsync(ulong userId, CancellationToken cancellationToken = default);
    Task<CartItem> AddItemToCartWithTransactionAsync(CartItem cartItem, CancellationToken cancellationToken = default);
    Task<CartItem?> FindItem(ulong cartId, ulong productId, CancellationToken cancellationToken = default);
    Task<CartItem> UpdateCartWithTransactionAsync(CartItem cartItem, CancellationToken cancellationToken = default);
    Task<bool> RemoveItemFromCartWithTransactionAsync(CartItem cartItem, CancellationToken cancellationToken = default);
    Task<bool> CheckIfProductAlreadyInCart(ulong cartId, ulong productId, CancellationToken cancellationToken = default);
}
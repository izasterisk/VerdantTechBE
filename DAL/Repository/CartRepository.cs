using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository;

public class CartRepository : ICartRepository
{
    private readonly IRepository<Cart> _cartRepository;
    private readonly IRepository<CartItem> _cartItemsRepository;
    private readonly VerdantTechDbContext _dbContext;
    private readonly IRepository<MediaLink> _mediaLinkRepository;
    
    public CartRepository(
        IRepository<Cart> cartRepository, 
        IRepository<CartItem> cartItemsRepository, 
        VerdantTechDbContext dbContext,
        IRepository<MediaLink> mediaLinkRepository)
    {
        _cartRepository = cartRepository;
        _cartItemsRepository = cartItemsRepository;
        _dbContext = dbContext;
        _mediaLinkRepository = mediaLinkRepository;
    }
    
    public async Task<Cart> CreateCartByUserIdWithTransactionAsync(ulong userId, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var cart = new Cart
            {
                CustomerId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var createdCart = await _cartRepository.CreateAsync(cart, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return createdCart;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
    
    public async Task<CartItem> AddItemToCartWithTransactionAsync(CartItem cartItem, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            cartItem.CreatedAt = DateTime.UtcNow;
            cartItem.UpdatedAt = DateTime.UtcNow;
            var createdCartItem = await _cartItemsRepository.CreateAsync(cartItem, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return createdCartItem;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<bool> DeleteItemFromCartWithTransactionAsync(CartItem item, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var deletedCartItem = await _cartItemsRepository.DeleteAsync(item, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return deletedCartItem;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<CartItem> UpdateCartWithTransactionAsync(CartItem cartItem, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            cartItem.UpdatedAt = DateTime.UtcNow;
            var updatedCartItem = await _cartItemsRepository.UpdateAsync(cartItem, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return updatedCartItem;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
    
    public async Task<Cart?> GetCartByUserIdWithRelationsAsync(ulong userId, CancellationToken cancellationToken = default)
    {
        return await _cartRepository.GetWithRelationsAsync(
            c => c.CustomerId == userId, 
            true,
            query => query
                .Include(c => c.CartItems).ThenInclude(ci => ci.Product)
                .Include(c => c.Customer),
            cancellationToken);
    }
    
    public async Task<List<MediaLink>> GetAllProductImagesForMultipleProducts(List<ulong> productIds, CancellationToken cancellationToken = default)
    {
        if (productIds == null || !productIds.Any())
        {
            return new List<MediaLink>();
        }

        return await _mediaLinkRepository.GetAllByFilterAsync(
            p => productIds.Contains(p.OwnerId) && p.OwnerType == MediaOwnerType.Products,
            true, cancellationToken);
    }

    public async Task<bool> CheckIfProductAlreadyInCart(ulong cartId, ulong productId, CancellationToken cancellationToken = default)
    {
        return await _cartItemsRepository.AnyAsync(c => c.CartId == cartId && c.ProductId == productId, cancellationToken);
    }
    
    public async Task<CartItem?> FindItem(ulong cartId, ulong productId, CancellationToken cancellationToken = default)
    {
        return await _cartItemsRepository.GetAsync(c => c.CartId == cartId && c.ProductId == productId, true, cancellationToken);
    }
}
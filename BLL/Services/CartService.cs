using AutoMapper;
using BLL.DTO.Cart;
using BLL.Interfaces;
using DAL.Data.Models;
using DAL.IRepository;

namespace BLL.Services;

public class CartService : ICartService
{
    private readonly IMapper _mapper;
    private readonly ICartRepository _cartRepository;
    
    public CartService(IMapper mapper, ICartRepository cartRepository)
    {
        _mapper = mapper;
        _cartRepository = cartRepository;
    }
    
    public async Task<CartResponseDTO> AddToCartAsync(ulong userId, CartAddDTO dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} is null");
        if(dto.Quantity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(dto.Quantity), "Số lượng sản phẩm phải lớn hơn 1.");
        }
        
        var cart = await _cartRepository.GetCartByUserIdWithRelationsAsync(userId, cancellationToken);
        if (cart == null)
        {
            cart = await _cartRepository.CreateCartByUserIdWithTransactionAsync(userId, cancellationToken);
        }
        else
        {
            var exists = await _cartRepository.CheckIfProductAlreadyInCart(cart.Id, dto.ProductId, cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException("Sản phẩm đã tồn tại trong giỏ hàng người dùng. Vui lòng sử dụng chức năng cập nhật.");
            }
        }
        var cartItem = _mapper.Map<CartItem>(dto);
        cartItem.CartId = cart.Id;
        await _cartRepository.AddItemToCartWithTransactionAsync(cartItem, cancellationToken);
        var updatedCart = await _cartRepository.GetCartByUserIdWithRelationsAsync(userId, cancellationToken);
        return _mapper.Map<CartResponseDTO>(updatedCart);
    }
    
    public async Task<CartResponseDTO> UpdateCartItemAsync(ulong userId, CartAddDTO dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} is null");
        
        var cart = await _cartRepository.GetCartByUserIdWithRelationsAsync(userId, cancellationToken);
        if (cart == null)
        {
            throw new InvalidOperationException("Giỏ hàng không tồn tại cho người dùng này.");
        }
        var item = await _cartRepository.FindItem(cart.Id, dto.ProductId, cancellationToken);
        if (item == null)
        {
            throw new InvalidOperationException("Sản phẩm không tồn tại trong giỏ hàng người dùng.");
        }

        if (dto.Quantity == 0)
        {
            var response = await _cartRepository.DeleteItemFromCartWithTransactionAsync(item, cancellationToken);
            if (!response)
            {
                throw new InvalidOperationException("Xoá sản phẩm khỏi giỏ hàng thất bại.");
            }
        }
        else
        {
            item.Quantity = dto.Quantity;
            await _cartRepository.UpdateCartWithTransactionAsync(item, cancellationToken);
        }
        var updatedCart = await _cartRepository.GetCartByUserIdWithRelationsAsync(userId, cancellationToken);
        return _mapper.Map<CartResponseDTO>(updatedCart);
    }
    
    public async Task<CartResponseDTO?> GetCartByUserIdAsync(ulong userId, CancellationToken cancellationToken = default)
    {
        var cart = await _cartRepository.GetCartByUserIdWithRelationsAsync(userId, cancellationToken);
        return cart == null ? null : _mapper.Map<CartResponseDTO>(cart);
    }
}
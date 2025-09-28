using Microsoft.AspNetCore.Mvc;
using BLL.Interfaces;
using BLL.DTO;
using BLL.DTO.Cart;
using System.Net;
using Microsoft.AspNetCore.Authorization;

namespace Controller.Controllers;

[Route("api/[controller]")]
public class CartController : BaseController
{
    private readonly ICartService _cartService;
    
    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    /// <summary>
    /// Thêm sản phẩm vào giỏ hàng
    /// </summary>
    /// <param name="dto">Thông tin sản phẩm cần thêm vào giỏ hàng</param>
    /// <returns>Thông tin giỏ hàng đã cập nhật</returns>
    [HttpPost("add")]
    [Authorize]
    [EndpointSummary("Add Product to Cart")]
    public async Task<ActionResult<APIResponse>> AddToCart([FromBody] CartAddDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var userId = GetCurrentUserId();
            var cart = await _cartService.AddToCartAsync(userId, dto, GetCancellationToken());
            return SuccessResponse(cart, HttpStatusCode.Created);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Cập nhật số lượng sản phẩm trong giỏ hàng
    /// </summary>
    /// <param name="dto">Thông tin sản phẩm cần cập nhật trong giỏ hàng</param>
    /// <returns>Thông tin giỏ hàng đã cập nhật</returns>
    [HttpPut("update")]
    [Authorize]
    [EndpointSummary("Update Cart Item Quantity")]
    [EndpointDescription("Nếu quantity = 0, sản phẩm sẽ bị xóa khỏi giỏ hàng.")]
    public async Task<ActionResult<APIResponse>> UpdateCartItem([FromBody] CartAddDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var userId = GetCurrentUserId();
            var cart = await _cartService.UpdateCartItemAsync(userId, dto, GetCancellationToken());
            return SuccessResponse(cart);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy thông tin giỏ hàng của người dùng hiện tại
    /// </summary>
    /// <returns>Thông tin giỏ hàng với danh sách sản phẩm</returns>
    [HttpGet]
    [Authorize]
    [EndpointSummary("Get User Cart")]
    public async Task<ActionResult<APIResponse>> GetCart()
    {
        try
        {
            var userId = GetCurrentUserId();
            var cart = await _cartService.GetCartByUserIdAsync(userId, GetCancellationToken());
            
            if (cart == null)
                return SuccessResponse(new { message = "Giỏ hàng trống", cartItems = new List<object>() });

            return SuccessResponse(cart);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}
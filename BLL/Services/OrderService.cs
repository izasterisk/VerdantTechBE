using AutoMapper;
using BLL.DTO.Address;
using BLL.DTO.Courier;
using BLL.DTO.Order;
using BLL.Helpers;
using BLL.Helpers.Order;
using BLL.Interfaces;
using BLL.Interfaces.Infrastructure;
using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BLL.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;
    private readonly IOrderDetailRepository _orderDetailRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly ICourierApiClient _courierApiClient;
    private readonly IProductRepository _productRepository;
    private readonly IMemoryCache _memoryCache;
    
    public OrderService(IOrderRepository orderRepository, IMapper mapper, IOrderDetailRepository orderDetailRepository, IAddressRepository addressRepository, ICourierApiClient courierApiClient, IProductRepository productRepository, IMemoryCache memoryCache)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
        _orderDetailRepository = orderDetailRepository;
        _addressRepository = addressRepository;
        _courierApiClient = courierApiClient;
        _productRepository = productRepository;
        _memoryCache = memoryCache;
    }

    public async Task<OrderPreviewResponseDTO> CreateOrderPreviewAsync(ulong userId, OrderPreviewCreateDTO dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} is null");
        var userExists = await _orderRepository.FindUserExistAsync(userId, cancellationToken);
        if (!userExists)
            throw new KeyNotFoundException($"Người dùng với ID {userId} không tồn tại.");
        var address = await _addressRepository.GetAddressByIdAsync(dto.AddressId, cancellationToken);
        if (address == null)
            throw new KeyNotFoundException($"Địa chỉ với ID {dto.AddressId} không tồn tại.");
        var addressBelongsToUser = await _orderRepository.ValidateAddressBelongsToUserAsync(dto.AddressId, userId, cancellationToken);
        if (!addressBelongsToUser)
            throw new ArgumentException($"Địa chỉ với ID {dto.AddressId} không thuộc về người dùng với ID {userId}.");

        OrderPreviewResponseDTO response = _mapper.Map<OrderPreviewResponseDTO>(dto);
        response.CustomerId = userId;
        response.Status = OrderStatus.Pending.ToString();
        
        List<OrderDetailPreviewCreateDTO> orderDetailsCreate = dto.OrderDetails.ToList();
        List<OrderDetailPreviewResponseDTO> orderDetailsUpdate = new List<OrderDetailPreviewResponseDTO>();
        decimal orderSubtotal = 0;
        decimal length = 0;
        decimal width = 0;
        decimal height = 0;
        decimal weight = 0;
        decimal cod = 0;
        foreach (var orderDetail in orderDetailsCreate)
        {
            var product = await _productRepository.GetProductByIdAsync(orderDetail.ProductId, true, cancellationToken);
            if (product == null)
                throw new KeyNotFoundException($"Sản phẩm với ID {orderDetail.ProductId} không tồn tại.");
            var productResponse = _mapper.Map<ProductResponseDTO>(product);
            if (product.DimensionsCm.TryGetValue("length", out var l))
                length = Math.Max(length, l * orderDetail.Quantity);
            if (product.DimensionsCm.TryGetValue("width", out var w))
                width = Math.Max(width, w);
            if (product.DimensionsCm.TryGetValue("height", out var h))
                height = Math.Max(height, h);
            if (product.WeightKg.HasValue)
                weight += product.WeightKg.Value * orderDetail.Quantity;
            var orderDetailResponse = new OrderDetailPreviewResponseDTO
            {
                Product = productResponse,
                Quantity = orderDetail.Quantity,
                DiscountAmount = orderDetail.DiscountAmount,
                Subtotal = OrderHelper.ComputeSubtotalForOrderItem(orderDetail.Quantity, product.UnitPrice, orderDetail.DiscountAmount)
            };
            orderSubtotal += orderDetailResponse.Subtotal;
            orderDetailsUpdate.Add(orderDetailResponse);
        }
        
        response.Subtotal = orderSubtotal;
        response.TotalAmountBeforeShippingFee = OrderHelper.ComputeTotalAmountForOrder(response.Subtotal, response.TaxAmount, 0, response.DiscountAmount);
        response.OrderDetails = orderDetailsUpdate;
        response.Address = _mapper.Map<AddressResponseDTO>(address);
        
        if (dto.OrderPaymentMethod == OrderPaymentMethod.COD)
            cod = response.TotalAmountBeforeShippingFee;
        List<RateResponseDTO> shippingOptions = await _courierApiClient.GetRatesAsync(720300, 700000, 
            address.CommuneCode, address.ProvinceCode, cod, response.TotalAmountBeforeShippingFee, 
            width, height, length, weight, cancellationToken);
        response.ShippingDetails = _mapper.Map<List<ShippingDetailDTO>>(shippingOptions);
        
        var cacheKey = OrderHelper.GenerateOrderPreviewCacheKey(response.orderPreviewId);
        var cacheOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
        _memoryCache.Set(cacheKey, response, cacheOptions);
        
        return response;
    }
    
    public async Task<OrderResponseDTO> CreateOrderAsync(Guid orderPreviewId, OrderCreateDTO dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} is null");
        OrderPreviewResponseDTO? preview = OrderHelper.GetOrderPreviewFromCache(_memoryCache, orderPreviewId);
        if (preview == null)
            throw new KeyNotFoundException($"Order preview với ID {orderPreviewId} không tồn tại hoặc đã hết hạn. Vui lòng tạo đơn hàng mới.");
        var userExists = await _orderRepository.FindUserExistAsync(preview.CustomerId, cancellationToken);
        if (!userExists)
            throw new KeyNotFoundException($"Người dùng với ID {preview.CustomerId} không tồn tại.");
        
        Order order = _mapper.Map<Order>(preview);
        var selectedShipping = preview.ShippingDetails.FirstOrDefault(s => s.Id == dto.ShippingDetailId);
        if (selectedShipping == null)
            throw new KeyNotFoundException($"Phương thức vận chuyển với ID {dto.ShippingDetailId} không tồn tại trong order preview.");
        order.ShippingFee = selectedShipping.TotalAmount;
        order.TotalAmount = OrderHelper.ComputeTotalAmountForOrder(order.Subtotal, order.TaxAmount, order.ShippingFee, order.DiscountAmount);
        order.ShippingMethod = selectedShipping.Service;
        order.AddressId = preview.Address.Id;
        List<OrderDetail> orderDetails = preview.OrderDetails.Select(od => new OrderDetail
        {
            ProductId = od.Product.Id,
            Quantity = od.Quantity,
            UnitPrice = od.Product.UnitPrice,
            DiscountAmount = od.DiscountAmount,
            Subtotal = od.Subtotal
        }).ToList();
        var createdOrder = await _orderRepository.CreateOrderWithTransactionAsync(order, orderDetails, cancellationToken);
        var finalResponse = await _orderRepository.GetOrderByIdAsync(createdOrder.Id, cancellationToken);
        return _mapper.Map<OrderResponseDTO>(finalResponse);
    }

    public async Task<OrderResponseDTO> UpdateOrderAsync(ulong orderId, OrderUpdateDTO dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} is null");
        
        var existingOrder = await _orderRepository.GetOrderByIdAsync(orderId, cancellationToken);
        if (existingOrder == null)
            throw new KeyNotFoundException($"Đơn hàng với ID {orderId} không tồn tại.");
        if (dto.CancelledReason != null)
        {
            if (dto.Status == null)
                dto.Status = OrderStatus.Cancelled;
            else
                throw new ArgumentException("Khi đã có CancelledReason, trạng thái đơn hàng nhận vào phải là null hoặc Cancelled.");
        }
        if (existingOrder.Status == OrderStatus.Shipped || existingOrder.Status == OrderStatus.Delivered)
        {
            if(dto.Status == OrderStatus.Cancelled)
                throw new InvalidOperationException($"Không thể hủy đơn khi hàng đã được vận chuyển đi.");
        }
        _mapper.Map(dto, existingOrder);
        var updatedOrder = await _orderRepository.UpdateOrderWithTransactionAsync(existingOrder, cancellationToken);
        return _mapper.Map<OrderResponseDTO>(updatedOrder);
    }
    
    public async Task<OrderResponseDTO?> GetOrderByOrderIdAsync(ulong orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetOrderByIdAsync(orderId, cancellationToken);
        return order == null ? null : _mapper.Map<OrderResponseDTO>(order);
    }
    
    public async Task<List<OrderResponseDTO>> GetAllOrdersByUserIdAsync(ulong userId, CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetAllOrdersByUserIdAsync(userId, cancellationToken);
        return _mapper.Map<List<OrderResponseDTO>>(orders);
    }
}
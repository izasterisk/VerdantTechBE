using AutoMapper;
using BLL.DTO;
using BLL.DTO.Address;
using BLL.DTO.Order;
using BLL.DTO.User;
using BLL.Helpers.Order;
using BLL.Interfaces;
using BLL.Interfaces.Infrastructure;
using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.Extensions.Caching.Memory;

namespace BLL.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;
    private readonly IOrderDetailRepository _orderDetailRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly IGoshipCourierApiClient _courierApiClient;
    private readonly IMemoryCache _memoryCache;
    
    public OrderService(IOrderRepository orderRepository, IMapper mapper, IOrderDetailRepository orderDetailRepository, IAddressRepository addressRepository, IGoshipCourierApiClient courierApiClient, IMemoryCache memoryCache)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
        _orderDetailRepository = orderDetailRepository;
        _addressRepository = addressRepository;
        _courierApiClient = courierApiClient;
        _memoryCache = memoryCache;
    }

    public async Task<OrderPreviewResponseDTO> CreateOrderPreviewAsync(ulong userId, OrderPreviewCreateDTO dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} is null");
        if (await _orderRepository.GetActiveUserByIdAsync(userId, cancellationToken) == null)
            throw new KeyNotFoundException($"Người dùng với ID {userId} không tồn tại.");
        var address = await _addressRepository.GetAddressByIdAsync(dto.AddressId, cancellationToken);
        if (address == null)
            throw new KeyNotFoundException($"Địa chỉ với ID {dto.AddressId} không tồn tại.");
        if(!await _orderRepository.ValidateAddressBelongsToUserAsync(dto.AddressId, userId, cancellationToken))
            throw new KeyNotFoundException($"Địa chỉ với ID {dto.AddressId} không thuộc về người dùng với ID {userId} hoặc đã bị xóa.");
        
        var response = _mapper.Map<OrderPreviewResponseDTO>(dto);
        response.CustomerId = userId;
        response.Address = _mapper.Map<AddressResponseDTO>(address);
    
        List<OrderDetailsPreviewResponseDTO> orderDetailsResponse = new();
        decimal subTotal = 0; decimal height = 0; decimal length = 0; decimal weight = 0; decimal width = 0;
        foreach (var orderDetail in dto.OrderDetails)
        {
            var productRaw = await _orderRepository.GetActiveProductByIdAsync(orderDetail.ProductId, cancellationToken);
            if (productRaw == null)
                throw new KeyNotFoundException($"Sản phẩm với ID {orderDetail.ProductId} không tồn tại hoặc đã bị xóa.");
            if (orderDetail.Quantity > productRaw.StockQuantity || productRaw.StockQuantity == 0)
                throw new InvalidOperationException($"Sản phẩm với ID {orderDetail.ProductId} không còn đủ hàng so với yêu cầu trong đơn hàng của bạn.");
            if (dto.OrderPaymentMethod == OrderPaymentMethod.Rent && productRaw.ForRent == false)
                throw new InvalidOperationException($"Sản phẩm với ID {orderDetail.ProductId} không thể thuê.");
            var product = _mapper.Map<ProductResponseDTO>(productRaw);
            product.Images = _mapper.Map<List<ProductImageResponseDTO>>(await _orderRepository.GetProductImagesByProductIdAsync(orderDetail.ProductId, cancellationToken));
            OrderDetailsPreviewResponseDTO orderDetailResponse = new()
            {
                Product = product,
                Quantity = orderDetail.Quantity,
                DiscountAmount = orderDetail.DiscountAmount,
                Subtotal = (product.UnitPrice * orderDetail.Quantity) - orderDetail.DiscountAmount
            };
            orderDetailsResponse.Add(orderDetailResponse);
            var l = product.DimensionsCm.GetValueOrDefault("length", 1m);
            var w = product.DimensionsCm.GetValueOrDefault("width", 1m);
            var h = product.DimensionsCm.GetValueOrDefault("height", 1m);
            (length, width, height) = OrderHelper.CalculatePackageDimensions(length, width, height, l, w, h, orderDetail.Quantity);
            subTotal += orderDetailResponse.Subtotal;
            weight += productRaw.WeightKg.GetValueOrDefault() * orderDetail.Quantity * 1000;
        }
        
        response.Subtotal = subTotal;
        response.TotalAmountBeforeShippingFee = subTotal + response.TaxAmount - response.DiscountAmount;
        response.OrderDetails = orderDetailsResponse;
        
        decimal cod = 0;
        if (dto.OrderPaymentMethod == OrderPaymentMethod.COD)
            cod = response.TotalAmountBeforeShippingFee;
        var availableServices = await _courierApiClient.GetRatesAsync("720300", "700000", 
            address.DistrictCode, address.ProvinceCode, cod, response.TotalAmountBeforeShippingFee, 
            width, height, length, weight, cancellationToken);
        response.ShippingDetails = _mapper.Map<List<ShippingDetailDTO>>(availableServices);
        OrderHelper.CacheOrderPreview(_memoryCache, response.OrderPreviewId, response);
        return response;
    }
    
    public async Task<OrderResponseDTO> CreateOrderAsync(Guid orderPreviewId, OrderCreateDTO dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} is null");
        var orderPreview = OrderHelper.GetOrderPreviewFromCache(_memoryCache, orderPreviewId);
        if (orderPreview == null)
            throw new KeyNotFoundException($"Đơn hàng xem trước với ID {orderPreviewId} đã hết hạn.");
        var selectedShipping = orderPreview.ShippingDetails.FirstOrDefault(s => s.Id == dto.CourierId);
        if (selectedShipping == null)
            throw new KeyNotFoundException($"Dịch vụ vận chuyển với ID {dto.CourierId} không tồn tại trong danh sách dịch vụ khả dụng.");
        
        List<OrderDetail> orderDetails = new();
        foreach (var orderDetail in orderPreview.OrderDetails)
        {
            orderDetails.Add(new OrderDetail
            {
                ProductId = orderDetail.Product.Id,
                Quantity = orderDetail.Quantity,
                UnitPrice = orderDetail.Product.UnitPrice,
                DiscountAmount = orderDetail.DiscountAmount,
                Subtotal = orderDetail.Subtotal
            });
        }
        var order = _mapper.Map<Order>(orderPreview);
        order.ShippingFee = selectedShipping.TotalAmount;
        order.TotalAmount = orderPreview.TotalAmountBeforeShippingFee + order.ShippingFee;
        order.AddressId = orderPreview.Address.Id;
        var createdOrder = await _orderRepository.CreateOrderWithTransactionAsync(order, orderDetails, cancellationToken);
        var response = await _orderRepository.GetOrderByIdAsync(createdOrder.Id, cancellationToken);
        if(response == null)
            throw new InvalidOperationException("Không thể tạo đơn hàng, vui lòng liên hệ với Staff để được hỗ trợ.");
        var finalResponse = _mapper.Map<OrderResponseDTO>(response);
        if (finalResponse.OrderDetails != null)
        {
            foreach (var product in finalResponse.OrderDetails)
            {
                product.Product.Images = _mapper.Map<List<ProductImageResponseDTO>>(await _orderRepository.GetProductImagesByProductIdAsync(product.Product.Id, cancellationToken));
            }
        }
        finalResponse.Customer = _mapper.Map<UserResponseDTO>(await _orderRepository.GetActiveUserByIdAsync(createdOrder.CustomerId, cancellationToken));
        finalResponse.Address = orderPreview.Address;
        OrderHelper.RemoveOrderPreviewFromCache(_memoryCache, orderPreviewId);
        return finalResponse;
    }
    
    public async Task<PagedResponse<OrderResponseDTO>> GetAllOrdersAsync(int page, int pageSize, String? status = null, CancellationToken cancellationToken = default)
    {
        var (orders, totalCount) = await _orderRepository.GetAllOrdersAsync(page, pageSize, status, cancellationToken);
        var response = _mapper.Map<List<OrderResponseDTO>>(orders);
        foreach (var item in response)
        {
            if (item.OrderDetails != null)
            {
                foreach (var product in item.OrderDetails)
                {
                    product.Product.Images = _mapper.Map<List<ProductImageResponseDTO>>(await _orderRepository.GetProductImagesByProductIdAsync(product.Product.Id, cancellationToken));
                }
            }
        }
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        return new PagedResponse<OrderResponseDTO>
        {
            Data = response,
            CurrentPage = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalRecords = totalCount,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }
}
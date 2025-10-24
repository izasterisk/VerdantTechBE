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
    private readonly IUserService _userService;
    
    public OrderService(IOrderRepository orderRepository, IMapper mapper, IOrderDetailRepository orderDetailRepository,
        IAddressRepository addressRepository, IGoshipCourierApiClient courierApiClient, IMemoryCache memoryCache,
        IUserService userService)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
        _orderDetailRepository = orderDetailRepository;
        _addressRepository = addressRepository;
        _courierApiClient = courierApiClient;
        _memoryCache = memoryCache;
        _userService = userService;
    }

    public async Task<OrderPreviewResponseDTO> CreateOrderPreviewAsync(ulong userId, OrderPreviewCreateDTO dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} rỗng.");
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
            weight += productRaw.WeightKg * orderDetail.Quantity * 1000; 
        }
        
        response.Subtotal = subTotal;
        response.TotalAmountBeforeShippingFee = subTotal + response.TaxAmount - response.DiscountAmount;
        response.OrderDetails = orderDetailsResponse;
        
        decimal cod = 0;
        if (dto.OrderPaymentMethod == OrderPaymentMethod.COD)
            cod = response.TotalAmountBeforeShippingFee;
        var fromAddress = await _addressRepository.GetAddressByIdAsync(1, cancellationToken);
        if (fromAddress == null)
            throw new KeyNotFoundException($"Địa chỉ gửi không tồn tại.");
        
        var (convertedWidth, convertedHeight, convertedLength, convertedWeight, convertedCod) = 
            OrderHelper.ConvertDimensionsToInt(width, height, length, weight, cod);
        var availableServices = await _courierApiClient.GetRatesAsync(fromAddress.DistrictCode, 
            fromAddress.ProvinceCode, address.DistrictCode, address.ProvinceCode, convertedCod, convertedWidth, 
            convertedHeight, convertedLength, convertedWeight, cancellationToken);
        response.Width = convertedWidth; response.Height = convertedHeight;
        response.Length = convertedLength; response.Weight = convertedWeight;
        response.ShippingDetails = _mapper.Map<List<ShippingDetailDTO>>(availableServices);
        OrderHelper.CacheOrderPreview(_memoryCache, response.OrderPreviewId, response);
        return response;
    }
    
    public async Task<OrderResponseDTO> CreateOrderAsync(Guid orderPreviewId, OrderCreateDTO dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} rỗng.");
        var orderPreview = OrderHelper.GetOrderPreviewFromCache(_memoryCache, orderPreviewId);
        if (orderPreview == null)
            throw new KeyNotFoundException($"Đơn hàng xem trước với ID {orderPreviewId} đã hết hạn.");
        var selectedShipping = orderPreview.ShippingDetails.FirstOrDefault(s => s.PriceTableId == dto.PriceTableId);
        if (selectedShipping == null)
            throw new KeyNotFoundException($"Dịch vụ vận chuyển với ID {dto.PriceTableId} không tồn tại trong danh sách dịch vụ khả dụng.");
        
        List<OrderDetail> orderDetails = new();
        List<Product> productsToUpdate = new();
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
            var productRaw = await _orderRepository.GetActiveProductByIdAsync(orderDetail.Product.Id, cancellationToken);
            if (productRaw == null)
                throw new KeyNotFoundException($"Sản phẩm với ID {orderDetail.Product.Id} không tồn tại hoặc đã bị xóa.");
            if (orderDetail.Quantity > productRaw.StockQuantity || productRaw.StockQuantity == 0)
                throw new InvalidOperationException($"Sản phẩm với ID {orderDetail.Product.Id} không còn đủ hàng so với yêu cầu của bạn. Vui lòng tạo đơn hàng mới.");
            productRaw.StockQuantity -= orderDetail.Quantity;
            productsToUpdate.Add(productRaw);
        }
        var order = _mapper.Map<Order>(orderPreview);
        order.ShippingFee = selectedShipping.TotalAmount;
        order.ShippingMethod = selectedShipping.Service;
        order.TotalAmount = orderPreview.TotalAmountBeforeShippingFee + order.ShippingFee;
        order.AddressId = orderPreview.Address.Id;
        order.CourierId = dto.PriceTableId;
        order.Width = orderPreview.Width; order.Height = orderPreview.Height;
        order.Length = orderPreview.Length; order.Weight = orderPreview.Weight;
        
        await _orderRepository.UpdateListProductWithTransactionAsync(productsToUpdate, cancellationToken);
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
    
    public async Task<OrderResponseDTO> ProcessOrderAsync(ulong orderId, OrderUpdateDTO dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} rỗng.");
        var order = await _orderRepository.GetOrderByIdAsync(orderId, cancellationToken);
        if (order == null)
            throw new KeyNotFoundException($"Đơn hàng với ID {orderId} không tồn tại.");
        if (dto.CancelledReason != null && dto.Status != OrderStatus.Cancelled)
            throw new InvalidOperationException("Khi cung cấp lý do hủy, trạng thái đơn hàng phải là 'Cancelled'.");
        OrderHelper.ValidateOrderStatusTransition(order.Status, dto.Status);
        if(dto.Status == OrderStatus.Processing)
            order.ConfirmedAt = DateTime.UtcNow;
        if(dto.Status == OrderStatus.Delivered)
            order.DeliveredAt = DateTime.UtcNow;
        if (dto.Status == OrderStatus.Cancelled)
        {
            order.CancelledAt = DateTime.UtcNow;
            order.CancelledReason = dto.CancelledReason;
        }
        if (dto.Status == OrderStatus.Paid)
        {
            // Validate thanh toán sẽ có sau này
        }
        if (dto.Status == OrderStatus.Shipped)
        {
            var from = await _userService.GetUserByIdAsync(1, cancellationToken);
            var to = await _userService.GetUserByIdAsync(order.CustomerId, cancellationToken);
            
            var targetAddress = to.Address.FirstOrDefault(a => a.Id == order.AddressId);
            if (targetAddress == null)
                throw new KeyNotFoundException("Địa chỉ không còn tồn tại.");
            to.Address.Insert(0, targetAddress); // Đưa địa chỉ này lên đầu tiên
            int payer = 1; int codAmount = 0;
            if (order.OrderPaymentMethod == OrderPaymentMethod.COD)
            {
                payer = 0; // Người gửi trả phí
                codAmount = (int)Math.Ceiling((double)order.TotalAmount);
            }
            order.TrackingNumber = await _courierApiClient.CreateShipmentAsync(from, to, codAmount, order.Length, 
                order.Width, order.Height, order.Weight, (int)Math.Ceiling((double)order.TotalAmount), payer, 
                order.CourierId, order.Notes ?? "" , cancellationToken);
        }
        order.Status = dto.Status;
        await _orderRepository.UpdateOrderWithTransactionAsync(order, cancellationToken);
        var response = await _orderRepository.GetOrderByIdAsync(orderId, cancellationToken);
        if(response == null)
            throw new InvalidOperationException("Không thể cập nhật đơn hàng, vui lòng liên hệ với Staff để được hỗ trợ.");
        var finalResponse = _mapper.Map<OrderResponseDTO>(response);
        if (finalResponse.OrderDetails != null)
        {
            foreach (var product in finalResponse.OrderDetails)
            {
                product.Product.Images = _mapper.Map<List<ProductImageResponseDTO>>(await _orderRepository.GetProductImagesByProductIdAsync(product.Product.Id, cancellationToken));
            }
        }
        finalResponse.Customer = _mapper.Map<UserResponseDTO>(await _orderRepository.GetActiveUserByIdAsync(response.CustomerId, cancellationToken));
        finalResponse.Address = _mapper.Map<AddressResponseDTO>(await _addressRepository.GetAddressByIdAsync(response.AddressId, cancellationToken));
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
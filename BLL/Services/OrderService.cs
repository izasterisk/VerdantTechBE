using AutoMapper;
using BLL.DTO;
using BLL.DTO.Address;
using BLL.DTO.Order;
using BLL.DTO.User;
using BLL.Helpers.Order;
using BLL.Interfaces;
using BLL.Interfaces.Infrastructure;
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
    private readonly IGHNCourierApiClient _ighnCourierApiClient;
    private readonly IMemoryCache _memoryCache;
    
    public OrderService(IOrderRepository orderRepository, IMapper mapper, IOrderDetailRepository orderDetailRepository, IAddressRepository addressRepository, IGHNCourierApiClient ighnCourierApiClient, IMemoryCache memoryCache)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
        _orderDetailRepository = orderDetailRepository;
        _addressRepository = addressRepository;
        _ighnCourierApiClient = ighnCourierApiClient;
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
        
        //Huhu I don't want this
        if(weight > 50000)
            weight = 50000;
        if(length > 200)
            length = 200;
        if(width > 200)
            width = 200;
        if(height > 200)
            height = 200;
        
        response.Subtotal = subTotal;
        response.TotalAmountBeforeShippingFee = subTotal + response.TaxAmount - response.DiscountAmount;
        response.OrderDetails = orderDetailsResponse;
        
        var availableServices = await _ighnCourierApiClient.GHNGetAvailableServicesAsync(3695, address.DistrictCode, cancellationToken);
        List<ShippingDetailDTO> shippingDetails = new();
        foreach (var availableService in availableServices)
        {
            try
            {
                int deliveryDate = await _ighnCourierApiClient.GHNGetDeliveryDateAsync(3695, "90752", 
                    address.DistrictCode, address.CommuneCode, availableService.ServiceId, cancellationToken);
                int shippingFee = await _ighnCourierApiClient.GHNGetShippingFeeAsync(3695, "90752", 
                    address.DistrictCode, address.CommuneCode, availableService.ServiceId, 
                    availableService.ServiceTypeId, (int)Math.Round(height, MidpointRounding.AwayFromZero), 
                    (int)Math.Round(length, MidpointRounding.AwayFromZero), 
                    (int)Math.Round(weight, MidpointRounding.AwayFromZero), 
                    (int)Math.Round(width, MidpointRounding.AwayFromZero), cancellationToken);
                shippingDetails.Add(new ShippingDetailDTO
                {
                    CourierServices = availableService,
                    ShippingFee = shippingFee,
                    EstimateDeliveryDate = OrderHelper.FromUnixTimestampToDateOnly(deliveryDate)
                });
            }
            catch
            {
                continue;
            }
        }
        if (shippingDetails.Count == 0)
            throw new InvalidOperationException("Không thể lấy được thông tin vận chuyển.");
        response.ShippingDetails = shippingDetails;
        OrderHelper.CacheOrderPreview(_memoryCache, response.OrderPreviewId, response);
        return response;
    }

    public async Task<OrderResponseDTO> CreateOrderAsync(Guid orderPreviewId, OrderCreateDTO dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} is null");
        var orderPreview = OrderHelper.GetOrderPreviewFromCache(_memoryCache, orderPreviewId);
        if (orderPreview == null)
            throw new KeyNotFoundException($"Đơn hàng xem trước với ID {orderPreviewId} đã hết hạn.");
        
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
        var selectedShipping = orderPreview.ShippingDetails.FirstOrDefault(s => s.CourierServices.ServiceId == dto.ServiceId);
        if (selectedShipping == null)
            throw new KeyNotFoundException($"Dịch vụ vận chuyển với ID {dto.ServiceId} không tồn tại trong đơn hàng xem trước.");
        var order = _mapper.Map<Order>(orderPreview);
        order.ShippingFee = selectedShipping.ShippingFee;
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
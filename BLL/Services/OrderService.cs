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
    private readonly IExportInventoryRepository _exportInventoryRepository;
    private readonly IUserRepository _userRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly INotificationService _notificationService;
    
    public OrderService(IOrderRepository orderRepository, IMapper mapper, IOrderDetailRepository orderDetailRepository,
        IAddressRepository addressRepository, IGoshipCourierApiClient courierApiClient, IMemoryCache memoryCache,
        IUserService userService, IExportInventoryRepository exportInventoryRepository, IUserRepository userRepository, 
        IWalletRepository walletRepository, INotificationService notificationService)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
        _orderDetailRepository = orderDetailRepository;
        _addressRepository = addressRepository;
        _courierApiClient = courierApiClient;
        _memoryCache = memoryCache;
        _userService = userService;
        _exportInventoryRepository = exportInventoryRepository;
        _userRepository = userRepository;
        _walletRepository = walletRepository;
        _notificationService = notificationService;
    }

    public async Task<OrderPreviewResponseDTO> CreateOrderPreviewAsync(ulong userId, OrderPreviewCreateDTO dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} rỗng.");
        await _userRepository.ValidateUserVerifiedAndActiveAsync(userId, cancellationToken);
        var address = await _addressRepository.GetAddressByIdAsync(dto.AddressId, cancellationToken);
        if (address == null)
            throw new KeyNotFoundException($"Địa chỉ với ID {dto.AddressId} không tồn tại.");
        if(!await _orderRepository.ValidateAddressBelongsToUserAsync(dto.AddressId, userId, cancellationToken))
            throw new KeyNotFoundException($"Địa chỉ với ID {dto.AddressId} không thuộc về người dùng với ID {userId} hoặc đã bị xóa.");
        
        var response = _mapper.Map<OrderPreviewResponseDTO>(dto);
        response.CustomerId = userId;
        response.Address = _mapper.Map<AddressResponseDTO>(address);
    
        List<OrderDetailsPreviewResponseDTO> orderDetailsResponse = new();
        var seen = new HashSet<ulong>();
        decimal subTotal = 0; decimal height = 0; decimal length = 0; decimal weight = 0; decimal width = 0;
        foreach (var orderDetail in dto.OrderDetails)
        {
            if (!seen.Add(orderDetail.ProductId))
                throw new InvalidOperationException($"Sản phẩm với ID {orderDetail.ProductId} bị lặp lại trong đơn hàng.");
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
        var user = await _userRepository.GetVerifiedAndActiveUserByIdAsync(orderPreview.CustomerId, cancellationToken);
        if (user == null)
            throw new KeyNotFoundException($"Người dùng với ID {orderPreview.CustomerId} không tồn tại hoặc đã bị xóa.");
        
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
        
        var createdOrder = await _orderRepository.CreateOrderWithTransactionAsync(order, orderDetails, productsToUpdate, cancellationToken);
        var response = await _orderRepository.GetOrderWithRelationsByIdAsync(createdOrder.Id, cancellationToken);
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
        
        finalResponse.Customer = _mapper.Map<UserResponseDTO>(user);
        finalResponse.Customer.UserAddresses.Insert(0, orderPreview.Address);
        OrderHelper.RemoveOrderPreviewFromCache(_memoryCache, orderPreviewId);
        return finalResponse;
    }

    public async Task<OrderResponseDTO> ShipOrderAsync(ulong staffId, ulong orderId, List<OrderDetailsShippingDTO> dtos, CancellationToken cancellationToken = default)
    {
        if(dtos == null || dtos.Count == 0)
            throw new ArgumentNullException($"{nameof(dtos)} rỗng.");
        var order = await _orderRepository.GetOrderWithRelationsByIdAsync(orderId, cancellationToken);
        if (order == null)
            throw new KeyNotFoundException($"Đơn hàng với ID {orderId} không tồn tại.");
        
        Dictionary<ulong, int> productQuantities = new();
        Dictionary<string, int> validateLotNumber = new(StringComparer.OrdinalIgnoreCase);
        List<ExportInventory> exportInventories = new();
        List<ProductSerial> exportSerials = new();
        var originalOrderMap = order.OrderDetails.ToDictionary(x => x.Id);
        foreach (var dto in dtos)
        {
            if (!originalOrderMap.TryGetValue(dto.OrderDetailId, out var detail))
                throw new InvalidOperationException($"OrderDetail ID {dto.OrderDetailId} không tồn tại trong đơn hàng này.");
            ulong? serial = null;
            if (dto.SerialNumber != null)
            {
                if(dto.Quantity != 1)
                    throw new InvalidOperationException("Với sản phẩm có số sê-ri, số lượng xuất phải là 1.");
                var s = await _orderDetailRepository.GetProductSerialAsync(detail.ProductId, dto.SerialNumber, dto.LotNumber, cancellationToken);
                if(s.Status != ProductSerialStatus.Stock)
                    throw new InvalidOperationException("Số sê-ri không đủ điều kiện để xuất kho.");
                serial = s.Id;
                exportSerials.Add(s);
            }
            else
            {
                if(await _orderDetailRepository.IsSerialRequiredByProductIdAsync(detail.ProductId, cancellationToken))
                    throw new InvalidOperationException($"Sản phẩm với ID {detail.ProductId} yêu cầu số sê-ri để xuất.");
                if (validateLotNumber.TryGetValue(dto.LotNumber, out var count))
                {
                    validateLotNumber[dto.LotNumber] = count + dto.Quantity;
                }
                else
                {
                    validateLotNumber[dto.LotNumber] = dto.Quantity;
                }
            }
            if (productQuantities.TryGetValue(dto.OrderDetailId, out var currentQuantity))
            {
                productQuantities[dto.OrderDetailId] = currentQuantity + dto.Quantity;
            }
            else
            {
                productQuantities.Add(dto.OrderDetailId, dto.Quantity);
            }
            exportInventories.Add(new ExportInventory
            {
                ProductId = detail.ProductId,
                ProductSerialId = serial,
                LotNumber = dto.LotNumber,
                OrderDetailId = detail.Id,
                Quantity = dto.Quantity,
                MovementType = MovementType.Sale,
                CreatedBy = staffId
            });
        }
        foreach (var kvp in productQuantities)
        {
            if (!originalOrderMap.TryGetValue(kvp.Key, out var detail))
                throw new InvalidOperationException($"OrderDetail ID {kvp.Key} không tồn tại trong đơn hàng này.");
            if (kvp.Value > detail.Quantity)
                throw new InvalidOperationException($"Số lượng xuất ({kvp.Value}) vượt quá số lượng đặt ({detail.Quantity}) cho ID {detail.ProductId}.");
        }
        foreach (var validate in validateLotNumber)
        {
            if (await _exportInventoryRepository.GetNumberOfProductLeftInInventoryThruLotNumberAsync(validate.Key, cancellationToken) < validate.Value)
                throw new InvalidOperationException($"Lô hàng với số lô {validate.Key} không còn đủ sản phẩm để xuất. Vui lòng kiểm tra lại.");
        }
        
        var from = await _userService.GetUserByIdAsync(1, cancellationToken);
        var to = _mapper.Map<UserResponseDTO>(await _userRepository.GetVerifiedAndActiveUserByIdAsync(order.CustomerId, cancellationToken));
        var targetAddress = await _addressRepository.GetAddressByIdAsync(order.AddressId, cancellationToken);
        if (targetAddress == null)
            throw new KeyNotFoundException("Địa chỉ không còn tồn tại.");
        to.UserAddresses.Insert(0, _mapper.Map<AddressResponseDTO>(targetAddress)); // Đưa địa chỉ này lên đầu tiên
        
        int payer = 1; int codAmount = 0;
        if (order.OrderPaymentMethod == OrderPaymentMethod.COD)
        {
            payer = 0; // Người gửi trả phí
            codAmount = (int)Math.Ceiling((double)order.TotalAmount);
        }
        order.TrackingNumber = await _courierApiClient.CreateShipmentAsync(from, to, codAmount, order.Length, 
            order.Width, order.Height, order.Weight, (int)Math.Ceiling((double)order.TotalAmount), payer, 
            order.CourierId, order.Notes ?? "" , cancellationToken);
        order.Status = OrderStatus.Shipped;
        
        await _orderRepository.UpdateOrderWithTransactionAsync(order, cancellationToken);
        await _exportInventoryRepository.CreateExportNUpdateProductSerialsWithTransactionAsync(exportInventories, ProductSerialStatus.Sold, exportSerials, cancellationToken);
        
        var finalResponse = _mapper.Map<OrderResponseDTO>(order);
        if (finalResponse.OrderDetails != null)
        {
            foreach (var product in finalResponse.OrderDetails)
            {
                product.Product.Images = _mapper.Map<List<ProductImageResponseDTO>>(await _orderRepository.GetProductImagesByProductIdAsync(product.Product.Id, cancellationToken));
            }
        }

        finalResponse.Customer = _mapper.Map<UserResponseDTO>(
                await _userRepository.GetVerifiedAndActiveUserByIdAsync(order.CustomerId, cancellationToken));
        finalResponse.Customer.UserAddresses.Insert(0, _mapper.Map<AddressResponseDTO>(await _addressRepository.GetAddressByIdAsync(order.AddressId, cancellationToken)));
        
        await _notificationService.CreateAndSendNotificationAsync(
            order.CustomerId,
            "Đơn hàng đã được gửi đi",
            $"Đơn hàng #{order.Id} của bạn đã được gửi đi. Mã vận đơn: {order.TrackingNumber}",
            NotificationReferenceType.Order,
            order.Id,
            cancellationToken);
        return finalResponse;
    }
    
    public async Task<OrderResponseDTO> ProcessOrderAsync(ulong userId, ulong orderId, OrderUpdateDTO dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} rỗng.");
        var order = await _orderRepository.GetOrderWithRelationsByIdAsync(orderId, cancellationToken);
        if (order == null)
            throw new KeyNotFoundException($"Đơn hàng với ID {orderId} không tồn tại.");
        var user = await _userRepository.GetVerifiedAndActiveUserByIdAsync(order.CustomerId, cancellationToken);
        if (user.Id != userId)
        {
            var check = await _userRepository.GetVerifiedAndActiveUserByIdAsync(userId, cancellationToken);
            if(check.Role != UserRole.Admin && check.Role != UserRole.Staff)
                throw new UnauthorizedAccessException("Bạn không có quyền cập nhật đơn hàng này.");
        }
        else
        {
            if(dto.Status != OrderStatus.Cancelled)
                throw new UnauthorizedAccessException("Khách hàng chỉ có thể hủy đơn hàng của mình.");
            if(order.Status != OrderStatus.Pending)
                throw new InvalidCastException("Chỉ những đơn hàng ở trạng thái 'Pending' mới có thể hủy bởi khách hàng.");
        }
        if (dto.CancelledReason != null && dto.Status != OrderStatus.Cancelled)
            throw new InvalidOperationException("Khi cung cấp lý do hủy, trạng thái đơn hàng phải là 'Cancelled'.");
        OrderHelper.ValidateOrderStatusTransition(order.Status, dto.Status);
        if (dto.Status == OrderStatus.Processing && order.Status == OrderStatus.Pending)
        {
            if(order.OrderPaymentMethod != OrderPaymentMethod.COD)
                throw new InvalidOperationException("Đơn hàng không phải COD không thể chuyển sang 'Processing' nếu chưa 'Paid'.");
            order.ConfirmedAt = DateTime.UtcNow;
        }
        if (dto.Status == OrderStatus.Delivered)
        {
            order.DeliveredAt = DateTime.UtcNow;
        }
        if (dto.Status == OrderStatus.Paid)
            throw new InvalidCastException("Không thể chuyển trạng thái đơn hàng thành 'Paid', đây là quá trình tự động.");
        var productsToUpdate = new List<Product>();
        if (dto.Status == OrderStatus.Cancelled)
        {
            foreach (var orderDetail in order.OrderDetails)
            {
                orderDetail.Product.StockQuantity += orderDetail.Quantity;
                productsToUpdate.Add(orderDetail.Product);
            }
            order.CancelledAt = DateTime.UtcNow;
            order.CancelledReason = dto.CancelledReason;
        }
        order.Status = dto.Status;
        await _orderRepository.UpdateOrderWithProductsTransactionAsync(order, productsToUpdate, cancellationToken);
        var response = await _orderRepository.GetOrderWithRelationsByIdAsync(orderId, cancellationToken);
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
        finalResponse.Customer = _mapper.Map<UserResponseDTO>(user);
        finalResponse.Customer.UserAddresses.Insert(0, _mapper.Map<AddressResponseDTO>(await _addressRepository.GetAddressByIdAsync(response.AddressId, cancellationToken)));
        
        var notificationMessage = dto.Status switch
        {
            OrderStatus.Processing => "Đơn hàng của bạn đang được xử lý.",
            OrderStatus.Delivered => "Đơn hàng của bạn đã được giao thành công.",
            OrderStatus.Cancelled => "Đơn hàng của bạn đã bị hủy.",
            _ => ""
        };
        await _notificationService.CreateAndSendNotificationAsync(
            order.CustomerId,
            "Đơn hàng của bạn vừa chuyển sang trạng thái mới.",
            notificationMessage,
            NotificationReferenceType.Order,
            order.Id,
            cancellationToken);
        return finalResponse;
    }
    
    public async Task<OrderResponseDTO> GetOrderByIdAsync(ulong orderId, CancellationToken cancellationToken = default)
    {
        var orderEntity = await _orderRepository.GetOrderWithRelationsByIdAsync(orderId, cancellationToken);
        if (orderEntity == null)
            throw new KeyNotFoundException($"Đơn hàng với ID {orderId} không tồn tại.");
        var item = _mapper.Map<OrderResponseDTO>(orderEntity);
        if (item.OrderDetails != null)
        {
            foreach (var product in item.OrderDetails)
            {
                product.Product.Images = _mapper.Map<List<ProductImageResponseDTO>>(await _orderRepository.GetProductImagesByProductIdAsync(product.Product.Id, cancellationToken));
            }
        }
        item.Customer = _mapper.Map<UserResponseDTO>(await _orderRepository.GetUserByIdAsync(orderEntity.CustomerId, cancellationToken));
        item.Customer.UserAddresses.Insert(0, _mapper.Map<AddressResponseDTO>(await _addressRepository.GetAddressByIdAsync(orderEntity.AddressId, cancellationToken)));
        return item;
    }
    
    public async Task<PagedResponse<OrderResponseDTO>> GetAllOrdersAsync(int page, int pageSize, String? status = null, CancellationToken cancellationToken = default)
    {
        var (orders, totalCount) = await _orderRepository.GetAllOrdersAsync(page, pageSize, status, cancellationToken);
        var response = new List<OrderResponseDTO>();
        
        foreach (var orderEntity in orders)
        {
            var item = _mapper.Map<OrderResponseDTO>(orderEntity);
            if (item.OrderDetails != null)
            {
                foreach (var product in item.OrderDetails)
                {
                    product.Product.Images = _mapper.Map<List<ProductImageResponseDTO>>(await _orderRepository.GetProductImagesByProductIdAsync(product.Product.Id, cancellationToken));
                }
            }
            item.Customer.UserAddresses.Insert(0, _mapper.Map<AddressResponseDTO>(orderEntity.Address));
            response.Add(item);
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

    public async Task<PagedResponse<OrderResponseDTO>> GetAllOrdersByUserIdAsync(ulong userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var (orders, totalCount) = await _orderRepository.GetAllOrdersByUserIdAsync(userId, page, pageSize, cancellationToken);
        var response = new List<OrderResponseDTO>();
        
        foreach (var orderEntity in orders)
        {
            var item = _mapper.Map<OrderResponseDTO>(orderEntity);
            if (item.OrderDetails != null)
            {
                foreach (var product in item.OrderDetails)
                {
                    product.Product.Images = _mapper.Map<List<ProductImageResponseDTO>>(await _orderRepository.GetProductImagesByProductIdAsync(product.Product.Id, cancellationToken));
                }
            }
            item.Customer.UserAddresses.Insert(0, _mapper.Map<AddressResponseDTO>(orderEntity.Address));
            response.Add(item);
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
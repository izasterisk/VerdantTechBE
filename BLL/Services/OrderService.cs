using AutoMapper;
using BLL.DTO.Order;
using BLL.Helpers.Order;
using BLL.Interfaces;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;
    private readonly IOrderDetailRepository _orderDetailRepository;
    
    public OrderService(IOrderRepository orderRepository, IMapper mapper, IOrderDetailRepository orderDetailRepository)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
        _orderDetailRepository = orderDetailRepository;
    }
    
    public async Task<OrderResponseDTO> CreateOrderAsync(ulong userId, OrderCreateDTO dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} is null");
        var userExists = await _orderRepository.FindUserExistAsync(userId, cancellationToken);
        if (!userExists)
            throw new KeyNotFoundException($"Người dùng với ID {userId} không tồn tại.");
        
        var addressBelongsToUser = await _orderRepository.ValidateAddressBelongsToUserAsync(dto.AddressId, userId, cancellationToken);
        if (!addressBelongsToUser)
            throw new ArgumentException($"Địa chỉ với ID {dto.AddressId} không thuộc về người dùng với ID {userId}. Vui lòng tạo địa chỉ mới.");
        
        Order order = _mapper.Map<Order>(dto);
        order.CustomerId = userId;
        
        List<OrderDetail> orderDetails = dto.OrderDetails
            .Select(odDto => _mapper.Map<OrderDetail>(odDto)).ToList();
        decimal orderSubtotal = 0;
        foreach (var orderDetail in orderDetails)
        {
            orderDetail.Subtotal = OrderHelper.ComputeSubtotalForOrderItem(orderDetail.Quantity, orderDetail.UnitPrice, orderDetail.DiscountAmount);
            orderSubtotal += orderDetail.Subtotal;
        }
        order.Subtotal = orderSubtotal;
        
        // Xử lý vận chuyển sẽ được update sau này
        order.ShippingFee = 0;
        
        order.TotalAmount = OrderHelper.ComputeTotalAmountForOrder(order.Subtotal, order.TaxAmount, order.ShippingFee, order.DiscountAmount);
        var createdOrder = await _orderRepository.CreateOrderWithTransactionAsync(order, orderDetails, cancellationToken);
        var response = await _orderRepository.GetOrderByIdAsync(createdOrder.Id, cancellationToken);
        return _mapper.Map<OrderResponseDTO>(response);
    }

    public async Task<OrderResponseDTO> UpdateOrderAsync(ulong orderId, OrderUpdateDTO dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} is null");
        
        var existingOrder = await _orderRepository.GetOrderByIdAsync(orderId, cancellationToken);
        if (existingOrder == null)
            throw new KeyNotFoundException($"Đơn hàng với ID {orderId} không tồn tại.");
        if (dto.AddressId != null)
        {
            var addressBelongsToUser = await _orderRepository.ValidateAddressBelongsToUserAsync(dto.AddressId.Value, existingOrder.CustomerId, cancellationToken);
            if (!addressBelongsToUser)
                throw new ArgumentException($"Địa chỉ với ID {dto.AddressId} không thuộc về người dùng với ID {existingOrder.CustomerId}. Vui lòng tạo địa chỉ mới hoặc thử địa chỉ khác.");
        }
        _mapper.Map(dto, existingOrder);
        
        if(dto.OrderDetails != null)
        {
            List<OrderDetailUpdateDTO> orderDetailUpdateDTOs = dto.OrderDetails.ToList();
            foreach (var orderDetailUpdateDTO in orderDetailUpdateDTOs)
            {
                if (orderDetailUpdateDTO.Id == 0)
                {
                    if (!orderDetailUpdateDTO.ProductId.HasValue || !orderDetailUpdateDTO.Quantity.HasValue || !orderDetailUpdateDTO.UnitPrice.HasValue)
                        throw new ArgumentException($"ProductId, Quantity, UnitPrice là bắt buộc khi tạo mới OrderDetail với ID {orderDetailUpdateDTO.Id}.");
                    var validateProductExists = await _orderDetailRepository.ValidateProductAlreadyExistsInOrderAsync(orderId, orderDetailUpdateDTO.ProductId.Value, cancellationToken);
                    if (validateProductExists)
                        throw new ArgumentException($"Sản phẩm với ID {orderDetailUpdateDTO.ProductId} đã tồn tại trong đơn hàng với ID {orderId}. Vui lòng cập nhật số lượng thay vì thêm mới.");
                    var orderDetailUpdate = _mapper.Map<OrderDetail>(orderDetailUpdateDTO);
                    orderDetailUpdate.Subtotal = OrderHelper.ComputeSubtotalForOrderItem(orderDetailUpdate.Quantity, orderDetailUpdate.UnitPrice, orderDetailUpdate.DiscountAmount);
                    orderDetailUpdate.OrderId = orderId;
                    await _orderDetailRepository.CreateOrderDetailAsync(orderDetailUpdate);
                }
                else
                {
                    var existingOrderDetail = await _orderDetailRepository.GetOrderDetailByIdAsync(orderDetailUpdateDTO.Id, cancellationToken);
                    if(existingOrderDetail != null && existingOrderDetail.OrderId != orderId)
                        throw new KeyNotFoundException($"Chi tiết đơn hàng với ID {orderDetailUpdateDTO.Id} không thuộc về đơn hàng với ID {orderId}.");
                    if(existingOrderDetail == null)
                        throw new KeyNotFoundException($"Chi tiết đơn hàng với ID {orderDetailUpdateDTO.Id} không tồn tại.");
                    var temp = existingOrderDetail;
                    _mapper.Map(orderDetailUpdateDTO, existingOrderDetail);
                    existingOrderDetail.Subtotal = OrderHelper.ComputeSubtotalForOrderItem(existingOrderDetail.Quantity, existingOrderDetail.UnitPrice, existingOrderDetail.DiscountAmount);
                
                    if (orderDetailUpdateDTO.Quantity == 0)
                    {
                        var deletedOrderDetail = await _orderDetailRepository.DeleteOrderDetailAsync(temp);
                        if (!deletedOrderDetail)
                            throw new DbUpdateException($"Xoá sản phầm trong đơn hàng với ID {orderDetailUpdateDTO.Id} không thành công.");
                        var check = await _orderDetailRepository.HasNoOrderDetailLeftAsync(orderId, cancellationToken);
                        if (check)
                        {
                            var deletedOrder = await _orderRepository.DeleteOrderWithTransactionAsync(existingOrder, cancellationToken);
                            if (!deletedOrder)
                            {
                                throw new DbUpdateException($"Xoá đơn hàng với ID {orderDetailUpdateDTO.Id} không thành công.");
                            }
                            throw new OrderHelper.OrderDeletedException(orderId);
                        }
                    }
                    else
                    {
                        var existingOrderDetailDTO = _mapper.Map<OrderDetailUpdateDTO>(temp);
                        var check = OrderHelper.AreOrderDetailsEqual(orderDetailUpdateDTO, existingOrderDetailDTO);
                        if (!check)
                        {
                            await _orderDetailRepository.UpdateOrderDetailAsync(existingOrderDetail);
                        }
                    }
                }
            }
        }
        
        var count = await _orderRepository.GetOrderByIdAsync(orderId, cancellationToken);
        existingOrder.Subtotal = 0;
        foreach (var detail in count.OrderDetails)
        {
            existingOrder.Subtotal += detail.Subtotal;
        }
        existingOrder.TotalAmount = OrderHelper.ComputeTotalAmountForOrder(existingOrder.Subtotal, existingOrder.TaxAmount, existingOrder.ShippingFee, existingOrder.DiscountAmount);
        var updatedOrder = await _orderRepository.UpdateOrderWithTransactionAsync(existingOrder, cancellationToken);
        var response = await _orderRepository.GetOrderByIdAsync(updatedOrder.Id, cancellationToken);
        return _mapper.Map<OrderResponseDTO>(response);
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
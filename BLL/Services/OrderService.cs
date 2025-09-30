using AutoMapper;
using BLL.DTO.Order;
using BLL.Helpers.Order;
using BLL.Interfaces;
using DAL.Data.Models;
using DAL.IRepository;

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
            throw new KeyNotFoundException($"Order with ID {orderId} not found.");
        _mapper.Map(dto, existingOrder);
        
        List<OrderDetailUpdateDTO> orderDetailUpdateDTOs = dto.OrderDetails.ToList();
        decimal orderSubtotal = 0;
        foreach (var orderDetailUpdateDTO in orderDetailUpdateDTOs)
        {
            var existingOrderDetail = await _orderDetailRepository.GetOrderDetailByIdAsync(orderDetailUpdateDTO.Id, cancellationToken);
            var orderDetailUpdate = _mapper.Map<OrderDetail>(orderDetailUpdateDTO);
            orderDetailUpdate.Subtotal = OrderHelper.ComputeSubtotalForOrderItem(orderDetailUpdate.Quantity, orderDetailUpdate.UnitPrice, orderDetailUpdate.DiscountAmount);
            if(existingOrderDetail == null)
            {
                orderDetailUpdate.OrderId = orderId;
                await _orderDetailRepository.CreateOrderDetailAsync(orderDetailUpdate);
            }
            if (orderDetailUpdateDTO.Quantity == 0)
            {
                var check = await _orderDetailRepository.HasFewerThanTwoOrderDetailsAsync(orderId, cancellationToken);
                var deletedOrderDetail = await _orderDetailRepository.DeleteOrderDetailAsync(existingOrderDetail);
                if (!deletedOrderDetail)
                    throw new Exception($"Xoá sản phầm trong đơn hàng với ID {orderDetailUpdateDTO.Id} không thành công.");
                if (check)
                {
                    var deletedOrder = await _orderRepository.DeleteOrderWithTransactionAsync(existingOrder, cancellationToken);
                    if (!deletedOrder)
                        throw new Exception($"Xoá đơn hàng với ID {orderDetailUpdateDTO.Id} không thành công.");
                    break;
                }
            }
            else
            {
                var existingOrderDetailDTO = _mapper.Map<OrderDetailUpdateDTO>(existingOrderDetail);
                var check = OrderHelper.AreOrderDetailsEqual(orderDetailUpdateDTO, existingOrderDetailDTO);
                if (!check)
                {
                    await _orderDetailRepository.UpdateOrderDetailAsync(orderDetailUpdate);
                }
                orderSubtotal += orderDetailUpdate.Subtotal;
            }
        }
        
        existingOrder.Subtotal = orderSubtotal;
        existingOrder.TotalAmount = OrderHelper.ComputeTotalAmountForOrder(existingOrder.Subtotal, existingOrder.TaxAmount, existingOrder.ShippingFee, existingOrder.DiscountAmount);
        var updatedOrder = await _orderRepository.UpdateOrderWithTransactionAsync(existingOrder, cancellationToken);
        var response = await _orderRepository.GetOrderByIdAsync(updatedOrder.Id, cancellationToken);
        return _mapper.Map<OrderResponseDTO>(response);
    }
    
    public async Task<OrderResponseDTO?> GetOrderByIdAsync(ulong orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetOrderByIdAsync(orderId, cancellationToken);
        return order == null ? null : _mapper.Map<OrderResponseDTO>(order);
    }
}
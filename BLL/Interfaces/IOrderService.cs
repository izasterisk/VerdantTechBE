using BLL.DTO.Order;

namespace BLL.Interfaces;

public interface IOrderService
{
    Task<OrderResponseDTO> CreateOrderAsync(ulong userId, OrderCreateDTO dto, CancellationToken cancellationToken = default);
    Task<OrderResponseDTO> UpdateOrderAsync(ulong orderId, OrderUpdateDTO dto, CancellationToken cancellationToken = default);
    Task<OrderResponseDTO?> GetOrderByOrderIdAsync(ulong orderId, CancellationToken cancellationToken = default);
    Task<List<OrderResponseDTO>> GetAllOrdersByUserIdAsync(ulong userId, CancellationToken cancellationToken = default);
}
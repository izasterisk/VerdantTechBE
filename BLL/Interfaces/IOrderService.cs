using BLL.DTO;
using BLL.DTO.Order;

namespace BLL.Interfaces;

public interface IOrderService
{
    Task<OrderPreviewResponseDTO> CreateOrderPreviewAsync(ulong userId, OrderPreviewCreateDTO dto, CancellationToken cancellationToken = default);
    
    Task<OrderResponseDTO> CreateOrderAsync(Guid orderPreviewId, OrderCreateDTO dto, CancellationToken cancellationToken = default);
    
    Task<PagedResponse<OrderResponseDTO>> GetAllOrdersAsync(int page, int pageSize, String? status = null, CancellationToken cancellationToken = default);
}
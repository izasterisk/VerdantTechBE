using BLL.DTO;
using BLL.DTO.Request;
using DAL.Data;

namespace BLL.Interfaces;

public interface IRequestService
{
    Task<RequestResponseDTO> CreateRequestAsync(ulong userId, RequestCreateDTO dto, CancellationToken cancellationToken = default);
    
    Task<RequestResponseDTO> ProcessRequestAsync(ulong staffId, ulong requestId, RequestUpdateDTO dto, CancellationToken cancellationToken = default);
    
    Task<RequestResponseDTO> GetRequestByIdAsync(ulong requestId, CancellationToken cancellationToken = default);
    
    Task<List<RequestResponseDTO>> GetAllRequestByUserIdAsync(ulong userId, CancellationToken cancellationToken = default);

    Task<PagedResponse<RequestResponseDTO>> GetAllRequestByFiltersAsync(int page, int pageSize, RequestType? requestType = null,
        RequestStatus? requestStatus = null, CancellationToken cancellationToken = default);
}
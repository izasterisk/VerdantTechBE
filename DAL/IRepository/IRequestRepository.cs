using DAL.Data;
using DAL.Data.Models;

namespace DAL.IRepository;

public interface IRequestRepository
{
    Task<Request> CreateRequestWithTransactionAsync(Request request, List<MediaLink>? mediaLinks, CancellationToken cancellationToken = default);
    Task<Request> GetRequestByIdWithRelationsAsync(ulong id, CancellationToken cancellationToken = default);
    Task<Request> GetRequestByIdAsync(ulong id, CancellationToken cancellationToken = default);
    Task<List<Request>> GetAllRequestByUserIdWithRelationsAsync(ulong userId, CancellationToken cancellationToken = default);
    Task<Request> UpdateRequestAsync(Request request, CancellationToken cancellationToken = default);
    Task<List<MediaLink>> GetAllImagesByRequestIdAsync(ulong requestId, CancellationToken cancellationToken = default);
    Task<(List<Request>, int totalCount)> GetAllRequestByFiltersAsync(int page, int pageSize, RequestType? requestType = null,
        RequestStatus? requestStatus = null, CancellationToken cancellationToken = default);
}
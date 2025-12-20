using DAL.Data;
using DAL.Data.Models;

namespace DAL.IRepository;

public interface IRequestRepository
{
    Task<Request> CreateRequestWithTransactionAsync(Request request, RequestMessage message, List<MediaLink>? mediaLinks, CancellationToken cancellationToken = default);
    Task<Request> GetRequestByIdWithRelationsAsync(ulong id, CancellationToken cancellationToken = default);
    Task<Request> GetRequestByIdAsync(ulong id, CancellationToken cancellationToken = default);
    Task<Request> GetRequestByIdWithMessagesAsync(ulong id, CancellationToken cancellationToken = default);
    Task<RequestMessage> GetRequestMessageByIdAsync(ulong id, CancellationToken cancellationToken = default);
    Task<(List<Request>, int totalCount)> GetAllRequestByUserIdWithRelationsAsync(ulong userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ulong> UpdateRequestWithTransactionAsync(ulong requestId, Request? request, RequestMessage? requestMessage, CancellationToken cancellationToken = default);
    Task<List<MediaLink>> GetAllImagesByRequestMessageIdAsync(ulong id, CancellationToken cancellationToken = default);
    Task<(List<Request>, int totalCount)> GetAllRequestByFiltersAsync(int page, int pageSize, RequestType? requestType = null,
        RequestStatus? requestStatus = null, CancellationToken cancellationToken = default);
    Task<RequestMessage> CreateRequestMessageAsync(ulong requestId, RequestMessage message, List<MediaLink>? mediaLinks, CancellationToken cancellationToken = default);
}
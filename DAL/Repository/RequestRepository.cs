using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DAL.Repository;

public class RequestRepository : IRequestRepository
{
    private readonly VerdantTechDbContext _dbContext;
    private readonly IRepository<Request> _requestRepository;
    private readonly IRepository<MediaLink> _mediaLinkRepository;
    private readonly IRepository<RequestMessage> _requestMessageRepository;
    
    public RequestRepository(VerdantTechDbContext dbContext, IRepository<Request> requestRepository,
        IRepository<MediaLink> mediaLinkRepository, IRepository<RequestMessage> requestMessageRepository)
    {
        _dbContext = dbContext;
        _requestRepository = requestRepository;
        _mediaLinkRepository = mediaLinkRepository;
        _requestMessageRepository = requestMessageRepository;
    }
    
    public async Task<Request> CreateRequestWithTransactionAsync(Request request, RequestMessage message, List<MediaLink>? mediaLinks, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            request.CreatedAt = DateTime.UtcNow;
            request.UpdatedAt = DateTime.UtcNow;
            var createdRequest = await _requestRepository.CreateAsync(request, cancellationToken);

            await CreateRequestMessageAsync(createdRequest.Id, message, mediaLinks, cancellationToken);
            
            await transaction.CommitAsync(cancellationToken);
            return createdRequest;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
    
    public async Task<RequestMessage> CreateRequestMessageAsync(ulong requestId, RequestMessage message, List<MediaLink>? mediaLinks, CancellationToken cancellationToken = default)
    {
        message.CreatedAt = DateTime.UtcNow;
        message.UpdatedAt = DateTime.UtcNow;
        message.RequestId = requestId;
        var requestMessage = await _requestMessageRepository.CreateAsync(message, cancellationToken);

        if (mediaLinks != null && mediaLinks.Count > 0)
        {
            int sort = 1;
            foreach (var mediaLink in mediaLinks)
            {
                mediaLink.OwnerType = MediaOwnerType.RequestMessage;
                mediaLink.OwnerId = requestMessage.Id;
                mediaLink.Purpose = MediaPurpose.None;
                mediaLink.SortOrder = sort++;
                mediaLink.CreatedAt = DateTime.UtcNow;
                mediaLink.UpdatedAt = DateTime.UtcNow;
                await _mediaLinkRepository.CreateAsync(mediaLink, cancellationToken);
            }
        }
        return requestMessage;
    }

    public async Task<ulong> UpdateRequestWithTransactionAsync(ulong requestId, Request? request, RequestMessage? requestMessage, CancellationToken cancellationToken = default)
    {
        if(request == null && requestMessage == null)
            throw new InvalidCastException("Cần có ít nhất một trong hai đối tượng Request hoặc RequestMessage để cập nhật.");
        
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            if(requestMessage != null)
            {
                requestMessage.UpdatedAt = DateTime.UtcNow;
                await _requestMessageRepository.UpdateAsync(requestMessage, cancellationToken);
            }
            if (request != null)
            {
                request.UpdatedAt = DateTime.UtcNow;
                await _requestRepository.UpdateAsync(request, cancellationToken);
            }
            
            await transaction.CommitAsync(cancellationToken);
            return requestId;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
    
    public async Task<RequestMessage> GetRequestMessageByIdAsync(ulong id, CancellationToken cancellationToken = default)
    {
        return await _requestMessageRepository.GetAsync(rm => rm.Id == id, true, cancellationToken)
            ?? throw new KeyNotFoundException($"RequestMessage với ID {id} không tồn tại.");
    }
    
    public async Task<Request> GetRequestByIdWithRelationsAsync(ulong id, CancellationToken cancellationToken = default)
    {
        return await _requestRepository.GetWithRelationsAsync(r => r.Id == id, true, 
            q => q.Include(u => u.User)
                .Include(u =>  u.ProcessedByNavigation)
                .Include(u => u.RequestMessages.OrderBy(rm => rm.Id))
                .ThenInclude(rm => rm.Staff), cancellationToken)
        ?? throw new KeyNotFoundException($"Request với ID {id} không tồn tại.");
    }
    
    public async Task<Request> GetRequestByIdWithMessagesAsync(ulong id, CancellationToken cancellationToken = default)
    {
        return await _requestRepository.GetWithRelationsAsync(r => r.Id == id, true, 
                   q => q.Include(u => u.RequestMessages.OrderBy(rm => rm.Id)), 
                   cancellationToken)
               ?? throw new KeyNotFoundException($"Request với ID {id} không tồn tại.");
    }
    
    public async Task<List<Request>> GetAllRequestByUserIdWithRelationsAsync(ulong userId, CancellationToken cancellationToken = default)
    {
        return await _requestRepository.GetAllWithRelationsByFilterAsync(r => r.UserId == userId, true, 
            q => q.Include(u => u.User)
                .Include(u =>  u.ProcessedByNavigation)
                .Include(u => u.RequestMessages.OrderBy(rm => rm.Id))
                .ThenInclude(rm => rm.Staff), cancellationToken);
    }
    
    public async Task<Request> GetRequestByIdAsync(ulong id, CancellationToken cancellationToken = default)
    {
        return await _requestRepository.GetAsync(r => r.Id == id, true, cancellationToken)
            ?? throw new KeyNotFoundException($"Request với ID {id} không tồn tại.");
    }
    
    public async Task<(List<Request>, int totalCount)> GetAllRequestByFiltersAsync(int page, int pageSize, 
        RequestType? requestType = null, RequestStatus? requestStatus = null, CancellationToken cancellationToken = default)
    {
        Expression<Func<Request, bool>> filter = r => true;
        
        // Apply RequestType filter
        if (requestType.HasValue)
        {
            filter = r => r.RequestType == requestType.Value;
        }
        
        // Apply RequestStatus filter (combine with existing filter)
        if (requestStatus.HasValue)
        {
            if (requestType.HasValue)
            {
                filter = r => r.RequestType == requestType.Value && r.Status == requestStatus.Value;
            }
            else
            {
                filter = r => r.Status == requestStatus.Value;
            }
        }

        return await _requestRepository.GetPaginatedWithRelationsAsync(
            page, 
            pageSize, 
            filter, 
            useNoTracking: true, 
            orderBy: query => query.OrderByDescending(r => r.UpdatedAt),
            includeFunc: query => query.Include(r => r.User).Include(r => r.ProcessedByNavigation),
            cancellationToken
        );
    }
    
    public async Task<List<MediaLink>> GetAllImagesByRequestMessageIdAsync(ulong id, CancellationToken cancellationToken = default)
    {
        var mediaLinks = await _mediaLinkRepository.GetAllByFilterAsync(ml => ml.OwnerType == MediaOwnerType.RequestMessage 
            && ml.OwnerId == id, true, cancellationToken);        
        return mediaLinks.OrderBy(ml => ml.SortOrder).ToList();
    }
}
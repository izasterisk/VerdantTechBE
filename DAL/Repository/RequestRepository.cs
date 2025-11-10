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
    
    public RequestRepository(VerdantTechDbContext dbContext, IRepository<Request> requestRepository, IRepository<MediaLink> mediaLinkRepository)
    {
        _dbContext = dbContext;
        _requestRepository = requestRepository;
        _mediaLinkRepository = mediaLinkRepository;
    }
    
    public async Task<Request> CreateRequestWithTransactionAsync(Request request, List<MediaLink>? mediaLinks, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            request.CreatedAt = DateTime.UtcNow;
            request.UpdatedAt = DateTime.UtcNow;
            var createdRequest = await _requestRepository.CreateAsync(request, cancellationToken);

            if (mediaLinks != null)
            {
                int sort = 1;
                foreach (var mediaLink in mediaLinks)
                {
                    mediaLink.OwnerType = MediaOwnerType.Request;
                    mediaLink.OwnerId = createdRequest.Id;
                    mediaLink.Purpose = MediaPurpose.None;
                    mediaLink.SortOrder = sort++;
                    mediaLink.CreatedAt = DateTime.UtcNow;
                    mediaLink.UpdatedAt = DateTime.UtcNow;
                    await _mediaLinkRepository.CreateAsync(mediaLink, cancellationToken);
                }
            }
            
            await transaction.CommitAsync(cancellationToken);
            return createdRequest;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Request> UpdateRequestAsync(Request request, CancellationToken cancellationToken = default)
    {
        request.UpdatedAt = DateTime.UtcNow;
        return await _requestRepository.UpdateAsync(request, cancellationToken);
    }
    
    public async Task<Request> GetRequestByIdWithRelationsAsync(ulong id, CancellationToken cancellationToken = default)
    {
        return await _requestRepository.GetWithRelationsAsync(r => r.Id == id, true, 
            q => q.Include(u => u.User)
                .Include(u =>  u.ProcessedByNavigation), cancellationToken)
        ?? throw new KeyNotFoundException($"Request với ID {id} không tồn tại.");
    }
    
    public async Task<List<Request>> GetAllRequestByUserIdWithRelationsAsync(ulong userId, CancellationToken cancellationToken = default)
    {
        return await _requestRepository.GetAllWithRelationsByFilterAsync(r => r.UserId == userId, true,
            q => q.Include(u => u.User)
                .Include(u => u.ProcessedByNavigation), cancellationToken);
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
    
    public async Task<List<MediaLink>> GetAllImagesByRequestIdAsync(ulong requestId, CancellationToken cancellationToken = default)
    {
        return await _mediaLinkRepository.GetAllByFilterAsync(ml => ml.OwnerType == MediaOwnerType.Request 
            && ml.OwnerId == requestId, true, cancellationToken);
    }
}
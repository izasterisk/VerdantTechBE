using AutoMapper;
using BLL.DTO;
using BLL.DTO.Request;
using BLL.Interfaces;
using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;

namespace BLL.Services;

public class RequestService : IRequestService
{
    private readonly IRequestRepository _requestRepository;
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;
    
    public RequestService(IRequestRepository requestRepository, IMapper mapper, IUserRepository userRepository)
    {
        _requestRepository = requestRepository;
        _mapper = mapper;
        _userRepository = userRepository;
    }
    
    public async Task<RequestResponseDTO> CreateRequestAsync(ulong userId, RequestCreateDTO dto, CancellationToken cancellationToken = default)
    {
        var request = _mapper.Map<Request>(dto);
        await _userRepository.GetVerifiedAndActiveUserByIdAsync(userId, cancellationToken);
        request.UserId = userId;
        request.Status = RequestStatus.Pending;
        var images = _mapper.Map<List<MediaLink>>(dto.Images);
        
        var created = await _requestRepository.CreateRequestWithTransactionAsync(request, images, cancellationToken);
        var responseDto = _mapper.Map<RequestResponseDTO>(await _requestRepository.GetRequestByIdWithRelationsAsync(created.Id, cancellationToken));
        if(dto.Images != null && dto.Images.Count > 0)
        {
            var imgs = await _requestRepository.GetAllImagesByRequestIdAsync(created.Id, cancellationToken);
            responseDto.Images = _mapper.Map<List<RequestImageDTO>>(imgs);
        }
        return responseDto;
    }
    
    public async Task<RequestResponseDTO> ProcessRequestAsync(ulong staffId, ulong requestId, RequestUpdateDTO dto, CancellationToken cancellationToken = default)
    {
        var request = await _requestRepository.GetRequestByIdAsync(requestId, cancellationToken);
        if(request.Status != RequestStatus.Pending && request.Status != RequestStatus.InReview)
            throw new InvalidOperationException("Chỉ có thể xử lý các yêu cầu ở trạng thái Pending hoặc InReview.");
        
        if(dto.Status == RequestStatus.Pending || dto.Status == RequestStatus.Completed)
            throw new InvalidOperationException("Không thể cập nhật trạng thái về Pending hoặc Completed.");
        if(dto.Status == RequestStatus.InReview && dto.ReplyNotes != null)
            throw new InvalidOperationException("Không thể thêm ghi chú trả lời khi cập nhật trạng thái thành InReview.");
        if(dto.Status == RequestStatus.Rejected || dto.Status == RequestStatus.Approved || dto.Status == RequestStatus.Completed || dto.Status == RequestStatus.Cancelled)
        {
            if(string.IsNullOrWhiteSpace(dto.ReplyNotes) || dto.ReplyNotes == null)
                throw new InvalidOperationException($"Ghi chú trả lời là bắt buộc khi cập nhật trạng thái thành {dto.Status}.");
            request.ReplyNotes = dto.ReplyNotes;
            request.ProcessedAt = DateTime.UtcNow;
            request.ProcessedBy = staffId;
        }
        request.Status = dto.Status;
        var updatedRequest = await _requestRepository.UpdateRequestAsync(request, cancellationToken);
        var responseDto = _mapper.Map<RequestResponseDTO>(await _requestRepository.GetRequestByIdWithRelationsAsync(updatedRequest.Id, cancellationToken));
        var imgs = await _requestRepository.GetAllImagesByRequestIdAsync(updatedRequest.Id, cancellationToken);
        responseDto.Images = _mapper.Map<List<RequestImageDTO>>(imgs);
        return responseDto;
    }
    
    public async Task<RequestResponseDTO> GetRequestByIdAsync(ulong requestId, CancellationToken cancellationToken = default)
    {
        var request = await _requestRepository.GetRequestByIdWithRelationsAsync(requestId, cancellationToken);
        var responseDto = _mapper.Map<RequestResponseDTO>(request);
        var imgs = await _requestRepository.GetAllImagesByRequestIdAsync(requestId, cancellationToken);
        responseDto.Images = _mapper.Map<List<RequestImageDTO>>(imgs);
        return responseDto;
    }
    
    public async Task<List<RequestResponseDTO>> GetAllRequestByUserIdAsync(ulong userId, CancellationToken cancellationToken = default)
    {
        var requests = await _requestRepository.GetAllRequestByUserIdWithRelationsAsync(userId, cancellationToken);
        var responseDtos = _mapper.Map<List<RequestResponseDTO>>(requests);
        foreach (var responseDto in responseDtos)
        {
            var imgs = await _requestRepository.GetAllImagesByRequestIdAsync(responseDto.Id, cancellationToken);
            responseDto.Images = _mapper.Map<List<RequestImageDTO>>(imgs);
        }
        return responseDtos;
    }
    
    public async Task<PagedResponse<RequestResponseDTO>> GetAllRequestByFiltersAsync(int page, int pageSize, 
        RequestType? requestType = null, RequestStatus? requestStatus = null, CancellationToken cancellationToken = default)
    {
        var (requests, totalCount) = await _requestRepository.GetAllRequestByFiltersAsync(page, pageSize, requestType, requestStatus, cancellationToken);
        var requestDtos = _mapper.Map<List<RequestResponseDTO>>(requests);
        
        // Get media links for each request
        foreach (var requestDto in requestDtos)
        {
            var mediaLinks = await _requestRepository.GetAllImagesByRequestIdAsync(requestDto.Id, cancellationToken);
            requestDto.Images = _mapper.Map<List<RequestImageDTO>>(mediaLinks);
        }
        
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        return new PagedResponse<RequestResponseDTO>
        {
            Data = requestDtos,
            CurrentPage = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalRecords = totalCount,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }
}
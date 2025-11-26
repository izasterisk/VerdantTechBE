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
    private readonly INotificationService _notificationService;
    
    public RequestService(IRequestRepository requestRepository, IMapper mapper, IUserRepository userRepository, INotificationService notificationService)
    {
        _requestRepository = requestRepository;
        _mapper = mapper;
        _userRepository = userRepository;
        _notificationService = notificationService;
    }
    
    public async Task<RequestResponseDTO> CreateRequestAsync(ulong userId, RequestCreateDTO dto, CancellationToken cancellationToken = default)
    {
        await _userRepository.GetVerifiedAndActiveUserByIdAsync(userId, cancellationToken);
        var request = _mapper.Map<Request>(dto);
        request.UserId = userId;
        request.Status = RequestStatus.Pending;
        var message = new RequestMessage
        {
            Description = dto.Description
        };
        var images = _mapper.Map<List<MediaLink>>(dto.Images);
        
        var created = await _requestRepository.CreateRequestWithTransactionAsync(request, message, images, cancellationToken);
        var responseDto = _mapper.Map<RequestResponseDTO>(await _requestRepository.GetRequestByIdWithRelationsAsync(created.Id, cancellationToken));
        if (responseDto.RequestMessages != null)
        {
            foreach (var requestMessage in responseDto.RequestMessages)
            {
                var i = await _requestRepository.GetAllImagesByRequestMessageIdAsync(requestMessage.Id, cancellationToken);
                requestMessage.Images = _mapper.Map<List<RequestImageDTO>>(i);
            }
        }
        return responseDto;
    }
    
    public async Task<RequestResponseDTO> ProcessRequestAsync(ulong staffId, ulong requestId, RequestProcessDTO dto, CancellationToken cancellationToken = default)
    {
        if(dto.RequestMessages == null && dto.Status == null)
            throw new InvalidOperationException("Phải có ít nhất một trong hai: ghi chú trả lời hoặc trạng thái yêu cầu để xử lý.");
        var request = await _requestRepository.GetRequestByIdAsync(requestId, cancellationToken);
        if(request.Status != RequestStatus.Pending && request.Status != RequestStatus.InReview)
            throw new InvalidOperationException("Chỉ có thể xử lý các yêu cầu ở trạng thái Pending hoặc InReview.");
        if(request.Status == dto.Status)
            throw new InvalidOperationException("Trạng thái mới phải khác với trạng thái hiện tại của yêu cầu.");
        if(dto.Status == RequestStatus.Pending || dto.Status == RequestStatus.Completed)
            throw new InvalidOperationException("Không thể cập nhật trạng thái về Pending hoặc Completed."); //Không về completed vì phải refund.
        if(dto.Status == RequestStatus.InReview && dto.RequestMessages != null)
            throw new InvalidOperationException("Không thể thêm ghi chú trả lời khi cập nhật trạng thái thành InReview.");
        
        RequestMessage? message = null;
        Request? requestToUpdate = null;
        if(dto.RequestMessages != null)
        {
            message = await _requestRepository.GetRequestMessageByIdAsync(dto.RequestMessages.Id, cancellationToken);
            message.ReplyNotes = dto.RequestMessages.ReplyNotes;
            message.StaffId = staffId;
        }
        if(dto.Status != null)
        {
            request.Status = dto.Status.Value;
            if(dto.Status is RequestStatus.Rejected or RequestStatus.Cancelled)
            {
                request.ProcessedBy = staffId;
                request.ProcessedAt = DateTime.UtcNow;
            }
            requestToUpdate = request;
        }
        var updatedRequestId = await _requestRepository.UpdateRequestWithTransactionAsync(request.Id, requestToUpdate, message, cancellationToken);
        
        var notificationMessage = dto.Status switch
        {
            RequestStatus.InReview => "Yêu cầu của bạn đang được xem xét.",
            RequestStatus.Approved => $"Yêu cầu của bạn đã được phê duyệt.",
            RequestStatus.Rejected => $"Yêu cầu của bạn đã bị từ chối.",
            RequestStatus.Cancelled => $"Yêu cầu của bạn đã bị hủy.",
            _ => "Yêu cầu của bạn đã được cập nhật."
        };
        await _notificationService.CreateAndSendNotificationAsync(
            request.UserId,
            "Cập nhật yêu cầu",
            notificationMessage,
            NotificationReferenceType.Request,
            updatedRequestId,
            cancellationToken
        );
        
        var responseDto = _mapper.Map<RequestResponseDTO>(await _requestRepository.GetRequestByIdWithRelationsAsync(updatedRequestId, cancellationToken));
        if (responseDto.RequestMessages != null)
        {
            foreach (var requestMessage in responseDto.RequestMessages)
            {
                var i = await _requestRepository.GetAllImagesByRequestMessageIdAsync(requestMessage.Id, cancellationToken);
                requestMessage.Images = _mapper.Map<List<RequestImageDTO>>(i);
            }
        }
        return responseDto;
    }
    
    public async Task<RequestResponseDTO> SendNewRequestMessageAsync(ulong userId, ulong requestId, RequestMessageCreateDTO dto, CancellationToken cancellationToken = default)
    {
        await _userRepository.ValidateUserVerifiedAndActiveAsync(userId, cancellationToken);
        var request = await _requestRepository.GetRequestByIdWithMessagesAsync(requestId, cancellationToken);
        if (request.UserId != userId)
            throw new UnauthorizedAccessException("Người dùng không có quyền gửi tin nhắn cho yêu cầu này.");
        if(request.RequestMessages.Count == 3)
            throw new InvalidOperationException("Đã đạt đến số lượng tin nhắn tối đa cho yêu cầu này.");
        foreach(var msg in request.RequestMessages)
        {
            if(msg.ReplyNotes == null)
                throw new InvalidOperationException("Bạn cần chờ hệ thống phản hồi trước khi gửi lời nhắn tiếp theo.");
        }
        
        var message = new RequestMessage
        {
            Description = dto.Description
        };
        var images = _mapper.Map<List<MediaLink>>(dto.Images);
        await _requestRepository.CreateRequestMessageAsync(requestId, message, images, cancellationToken);
        
        var responseDto = _mapper.Map<RequestResponseDTO>(await _requestRepository.GetRequestByIdWithRelationsAsync(requestId, cancellationToken));
        if (responseDto.RequestMessages != null)
        {
            foreach (var requestMessage in responseDto.RequestMessages)
            {
                var i = await _requestRepository.GetAllImagesByRequestMessageIdAsync(requestMessage.Id, cancellationToken);
                requestMessage.Images = _mapper.Map<List<RequestImageDTO>>(i);
            }
        }
        return responseDto;
    }
    
    public async Task<RequestResponseDTO> GetRequestByIdAsync(ulong requestId, CancellationToken cancellationToken = default)
    {
        var responseDto = _mapper.Map<RequestResponseDTO>(await _requestRepository.GetRequestByIdWithRelationsAsync(requestId, cancellationToken));
        if (responseDto.RequestMessages != null)
        {
            foreach (var requestMessage in responseDto.RequestMessages)
            {
                var i = await _requestRepository.GetAllImagesByRequestMessageIdAsync(requestMessage.Id, cancellationToken);
                requestMessage.Images = _mapper.Map<List<RequestImageDTO>>(i);
            }
        }
        return responseDto;
    }
    
    public async Task<PagedResponse<RequestResponseDTO>> GetAllRequestByUserIdAsync(ulong userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var (requests, totalCount) = await _requestRepository.GetAllRequestByUserIdWithRelationsAsync(userId, page, pageSize, cancellationToken);
        var responseDtos = _mapper.Map<List<RequestResponseDTO>>(requests);
        
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        return new PagedResponse<RequestResponseDTO>
        {
            Data = responseDtos,
            CurrentPage = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalRecords = totalCount,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }
    
    public async Task<PagedResponse<RequestResponseDTO>> GetAllRequestByFiltersAsync(int page, int pageSize, 
        RequestType? requestType = null, RequestStatus? requestStatus = null, CancellationToken cancellationToken = default)
    {
        var (requests, totalCount) = await _requestRepository.GetAllRequestByFiltersAsync(page, pageSize, requestType, requestStatus, cancellationToken);
        var requestDtos = _mapper.Map<List<RequestResponseDTO>>(requests);
        
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
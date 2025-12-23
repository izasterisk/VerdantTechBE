using AutoMapper;
using BLL.DTO;
using BLL.DTO.CustomerVendorConversation;
using BLL.DTO.MediaLink;
using BLL.DTO.User;
using BLL.Helpers;
using BLL.Interfaces;
using BLL.Interfaces.Infrastructure;
using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.Extensions.Caching.Memory;

namespace BLL.Services;

public class CustomerVendorConversationsService : ICustomerVendorConversationsService
{
    private readonly ICustomerVendorConversationsRepository _customerVendorConversationsRepository;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;
    private readonly IChatHub _chatHub;
    private readonly IMemoryCache _cache;
    
    public CustomerVendorConversationsService(ICustomerVendorConversationsRepository customerVendorConversationsRepository,
        ICloudinaryService cloudinaryService, IMapper mapper, IUserRepository userRepository, IChatHub chatHub,
        IMemoryCache cache)
    {
        _customerVendorConversationsRepository = customerVendorConversationsRepository;
        _cloudinaryService = cloudinaryService;
        _mapper = mapper;
        _userRepository = userRepository;
        _chatHub = chatHub;
        _cache = cache;
    }
    
    public async Task<CustomerVendorMessageResponseDTO> SendNewMessageAsync(ulong userId, UserRole role, 
        CustomerVendorMessageCreateDTO dto, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"conversation:{dto.CustomerId}:{dto.VendorId}";
        bool isNewlyCreated = false;
        var conversation = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            var (conv, wasCreated) = await _customerVendorConversationsRepository.GetOrCreateConversationByUserIdAsync
                (dto.CustomerId, dto.VendorId, cancellationToken);
            isNewlyCreated = wasCreated;
            return conv;
        });        
        if (isNewlyCreated && role != UserRole.Customer)
            throw new InvalidOperationException("Chỉ khách hàng mới có thể bắt đầu cuộc trò chuyện với nhà cung cấp.");
        
        var message = new CustomerVendorMessage
        {
            ConversationId = conversation!.Id,
            SenderType = role == UserRole.Customer ? CustomerVendorSenderType.Customer : CustomerVendorSenderType.Vendor,
            MessageText = dto.MessageText,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        var mediaLinks = new List<MediaLink>();
        if (dto.Images?.Count > 0)
        {
            if (dto.Images.Count > 3)
                throw new InvalidOperationException("Chỉ được upload tối đa 3 ảnh cho mỗi tin nhắn.");
            
            var uploadedImages = await Utils.UploadImagesAsync(
                _cloudinaryService,
                dto.Images,
                "customer-vendor-chat/messages",
                "none",
                1,
                cancellationToken
            );
            foreach (var img in uploadedImages)
            {
                mediaLinks.Add(new MediaLink
                {
                    // OwnerId 
                    OwnerType = MediaOwnerType.CustomerVendorMessages,
                    ImageUrl = img.ImageUrl,
                    ImagePublicId = img.ImagePublicId,
                    Purpose = MediaPurpose.None,
                    SortOrder = img.SortOrder
                });
            }
        }
        
        await _customerVendorConversationsRepository.SendNewMessageAsync(conversation, message, mediaLinks, cancellationToken);
        var response = _mapper.Map<CustomerVendorMessageResponseDTO>
            (await _customerVendorConversationsRepository.GetNewestMessageByConversationIdAsync(conversationId, cancellationToken));
        response.Images = _mapper.Map<List<MediaLinkItemDTO>>
            (await _customerVendorConversationsRepository.GetAllMessageImagesByIdAsync(response.Id, cancellationToken));
        
        // Gửi tin nhắn realtime qua SignalR
        await _chatHub.SendMessageToConversation(dto.CustomerId, dto.VendorId, response);
        return response;
    }
    
    public async Task<PagedResponse<CustomerVendorMessageResponseDTO>> GetAllMessagesByConversationIdAsync(ulong conversationId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var (messages, totalCount) = await _customerVendorConversationsRepository
            .GetAllMessagesByConversationIdAsync(conversationId, page, pageSize, cancellationToken);
        
        var messageIds = messages.Select(m => m.Id).ToList();
        
        var mediaLinks = await _customerVendorConversationsRepository
            .GetMediaLinksByOwnerIdsAsync(messageIds, cancellationToken);
        
        var mediaLinksDictionary = mediaLinks
            .GroupBy(ml => ml.OwnerId)
            .ToDictionary(g => g.Key, g => g.ToList());
        
        var messageDtos = _mapper.Map<List<CustomerVendorMessageResponseDTO>>(messages);
        
        foreach (var messageDto in messageDtos)
        {
            if (mediaLinksDictionary.TryGetValue(messageDto.Id, out var links))
            {
                messageDto.Images = _mapper.Map<List<MediaLinkItemDTO>>(links);
            }
        }
        
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        return new PagedResponse<CustomerVendorMessageResponseDTO>
        {
            Data = messageDtos,
            CurrentPage = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalRecords = totalCount,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }
    
    public async Task<PagedResponse<CustomerVendorConversationReponseDTO>> GetAllConversationsByUserIdAsync(ulong userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var (conversations, totalCount) = await _customerVendorConversationsRepository
            .GetAllConversationsByUserIdAsync(userId, page, pageSize, cancellationToken);
        
        var conversationDtos = _mapper.Map<List<CustomerVendorConversationReponseDTO>>(conversations);
        
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        return new PagedResponse<CustomerVendorConversationReponseDTO>
        {
            Data = conversationDtos,
            CurrentPage = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalRecords = totalCount,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }
}
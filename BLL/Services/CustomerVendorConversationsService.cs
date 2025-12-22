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

namespace BLL.Services;

public class CustomerVendorConversationsService : ICustomerVendorConversationsService
{
    private readonly ICustomerVendorConversationsRepository _customerVendorConversationsRepository;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;
    
    public CustomerVendorConversationsService(ICustomerVendorConversationsRepository customerVendorConversationsRepository,
        ICloudinaryService cloudinaryService, IMapper mapper, IUserRepository userRepository)
    {
        _customerVendorConversationsRepository = customerVendorConversationsRepository;
        _cloudinaryService = cloudinaryService;
        _mapper = mapper;
        _userRepository = userRepository;
    }
    
    public async Task<CustomerVendorConversationReponseDTO> CreateConversationAsync(ulong customerId, 
        CustomerVendorConversationCreateDTO dto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetVerifiedAndActiveUserByIdAsync(customerId, cancellationToken);
        if(user.Role != UserRole.Customer)
            throw new InvalidOperationException("Chỉ người dùng với vai trò Customer mới có thể khởi tạo cuộc trò chuyện với nhà cung cấp.");
        var vendor = await _userRepository.GetVerifiedAndActiveUserByIdAsync(dto.VendorId, cancellationToken);
        if(vendor.Role != UserRole.Vendor)
            throw new InvalidOperationException("Chỉ Vendor mới có thể nhận tin nhắn.");
        
        var conversation = new CustomerVendorConversation
        {
            CustomerId = customerId,
            VendorId = dto.VendorId
        };
        var message = new CustomerVendorMessage
        {
            SenderType = CustomerVendorSenderType.Customer,
            MessageText = dto.InitialMessage.MessageText
        };

        var mediaLinks = new List<MediaLink>();
        if (dto.InitialMessage.Images?.Count > 0)
        {
            if (dto.InitialMessage.Images.Count > 3)
                throw new InvalidOperationException("Chỉ được upload tối đa 3 ảnh cho mỗi tin nhắn.");
            
            var uploadedImages = await Utils.UploadImagesAsync(
                _cloudinaryService,
                dto.InitialMessage.Images,
                "customer-vendor-chat/messages",
                "none",
                1,
                cancellationToken
            );
            foreach (var img in uploadedImages)
            {
                mediaLinks.Add(new MediaLink
                {
                    OwnerType = MediaOwnerType.CustomerVendorMessages,
                    ImageUrl = img.ImageUrl,
                    ImagePublicId = img.ImagePublicId,
                    Purpose = MediaPurpose.None,
                    SortOrder = img.SortOrder
                });
            }
        }
        
        await _customerVendorConversationsRepository.CreateConversationAsync(
            conversation, 
            message, 
            mediaLinks, 
            cancellationToken
        );
        var createdConversation = await _customerVendorConversationsRepository
            .GetConversationWithRelationByIdAsync(conversation.Id, cancellationToken);

        var response = _mapper.Map<CustomerVendorConversationReponseDTO>(createdConversation);
        response.Customer = _mapper.Map<UserResponseDTO>(user);
        response.Vendor = _mapper.Map<UserResponseDTO>(vendor);
        foreach (var customerVendorMessage in response.CustomerVendorMessages)
        {
            customerVendorMessage.Images = _mapper.Map<List<MediaLinkItemDTO>>
                (await _customerVendorConversationsRepository.GetAllMessageImagesByIdAsync(customerVendorMessage.Id, cancellationToken));
        }
        return response;
    }
    
    public async Task<CustomerVendorMessageResponseDTO> SendNewMessageAsync(ulong userId, UserRole role, 
        ulong conversationId, CustomerVendorMessageCreateDTO dto, CancellationToken cancellationToken = default)
    {
        var conversation = await _customerVendorConversationsRepository.GetConversationByIdAsync(conversationId, cancellationToken);
        if (conversation.CustomerId != userId && conversation.VendorId != userId)
            throw new UnauthorizedAccessException("Người dùng không có quyền gửi tin nhắn trong cuộc trò chuyện này.");
        if(role != UserRole.Customer && role != UserRole.Vendor)
            throw new InvalidOperationException("Chỉ người dùng với vai trò Customer hoặc Vendor mới có thể gửi tin nhắn trong cuộc trò chuyện này.");
        
        var message = new CustomerVendorMessage
        {
            ConversationId = conversationId,
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
    
    public async Task<PagedResponse<CustomerVendorListConversationsReponseDTO>> GetAllConversationsByUserIdAsync(ulong userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var (conversations, totalCount) = await _customerVendorConversationsRepository
            .GetAllConversationsByUserIdAsync(userId, page, pageSize, cancellationToken);
        
        var conversationDtos = _mapper.Map<List<CustomerVendorListConversationsReponseDTO>>(conversations);
        
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        return new PagedResponse<CustomerVendorListConversationsReponseDTO>
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
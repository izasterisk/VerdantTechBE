using AutoMapper;
using BLL.DTO.CustomerVendorConversation;
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
    
    public CustomerVendorConversationsService(
        ICustomerVendorConversationsRepository customerVendorConversationsRepository,
        ICloudinaryService cloudinaryService,
        IMapper mapper)
    {
        _customerVendorConversationsRepository = customerVendorConversationsRepository;
        _cloudinaryService = cloudinaryService;
        _mapper = mapper;
    }
    
    public async Task<CustomerVendorConversationReponseDTO> CreateConversationAsync(ulong customerId, 
        CustomerVendorConversationCreateDTO dto, CancellationToken cancellationToken = default)
    {
        if (dto.InitialMessage.Images != null && dto.InitialMessage.Images.Count > 3)
            throw new InvalidOperationException("Chỉ được upload tối đa 3 ảnh cho mỗi tin nhắn.");
        if(dto.InitialMessage.SenderType == CustomerVendorSenderType.Vendor)
            throw new InvalidOperationException("Chỉ người dùng mới có thể khởi tạo cuộc trò chuyện với nhà cung cấp.");

        var conversation = new CustomerVendorConversation
        {
            CustomerId = customerId,
            VendorId = dto.VendorId
        };
        var message = new CustomerVendorMessage
        {
            SenderType = dto.InitialMessage.SenderType,
            MessageText = dto.InitialMessage.MessageText
        };

        // Upload ảnh nếu có
        var mediaLinks = new List<MediaLink>();
        if (dto.InitialMessage.Images != null && dto.InitialMessage.Images.Count > 0)
        {
            var uploadedImages = await Utils.UploadImagesAsync(
                _cloudinaryService,
                dto.InitialMessage.Images,
                "customer-vendor-chat/messages",
                "none",
                1,
                cancellationToken
            );

            // Convert to MediaLink entities
            // Note: OwnerId sẽ được set trong repository sau khi message được tạo
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

        // Gọi repository để tạo conversation, message và upload ảnh trong transaction
        await _customerVendorConversationsRepository.CreateConversationAsync(
            conversation, 
            message, 
            mediaLinks, 
            cancellationToken
        );

        // Lấy lại conversation với relations
        var createdConversation = await _customerVendorConversationsRepository
            .GetConversationWithRelationByIdAsync(conversation.Id, cancellationToken);

        // Map to response DTO
        var response = _mapper.Map<CustomerVendorConversationReponseDTO>(createdConversation);
        
        return response;
    }
}
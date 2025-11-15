using AutoMapper;
using BLL.DTO;
using BLL.DTO.ChatbotConversations;
using BLL.DTO.User;
using BLL.Interfaces;
using DAL.Data.Models;
using DAL.IRepository;

namespace BLL.Services;

public class ChatbotConversationService : IChatbotConversationService
{
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;
    private readonly IChatbotConversationRepository _chatbotConversationRepository;
    
    public ChatbotConversationService(IMapper mapper, IUserRepository userRepository, IChatbotConversationRepository chatbotConversationRepository)
    {
        _mapper = mapper;
        _userRepository = userRepository;
        _chatbotConversationRepository = chatbotConversationRepository;
    }
    
    public async Task<SendNewMessageResponseDTO> CreateNewChatbotConversationAsync(ulong userId, ChatbotMessageCreateDTO dto, CancellationToken cancellationToken = default)
    {
        await _userRepository.GetVerifiedAndActiveUserByIdAsync(userId, cancellationToken);
        var newConversation = new ChatbotConversation
        {
            CustomerId = userId,
        };
        var createdConversation = 
            await _chatbotConversationRepository.CreateConversationWithTransactionAsync(newConversation, 
            _mapper.Map<ChatbotMessage>(dto), cancellationToken);
        var response = new SendNewMessageResponseDTO
        {
            Conversation = _mapper.Map<ChatbotConversationsResponseDTO>(createdConversation.conversation),
            Message = _mapper.Map<ChatbotMessagesResponseDTO>(createdConversation.message)
        };
        return response;
    }
    
    public async Task<ChatbotMessagesResponseDTO> SendNewMessageAsync(ulong userId, ulong conversationId, ChatbotMessageCreateDTO dto, CancellationToken cancellationToken = default)
    {
        if (!await _chatbotConversationRepository.IsConversationBelongToUserAsync(conversationId, userId, cancellationToken))
            throw new KeyNotFoundException("Cuộc trò chuyện đã đóng hoặc người dùng đã bị xóa.");
        
        var newMessage = _mapper.Map<ChatbotMessage>(dto);
        newMessage.ConversationId = conversationId;
        var createdMessage = await _chatbotConversationRepository.CreateNewChatbotMessageAsync(newMessage, cancellationToken);
        return _mapper.Map<ChatbotMessagesResponseDTO>(createdMessage);
    }
    
    public async Task<ChatbotConversationsResponseDTO> UpdateChatbotConversationAsync(ulong conversationId, CancellationToken cancellationToken = default)
    {
        var conversation = await _chatbotConversationRepository.UpdateChatbotConversationAsync(await
            _chatbotConversationRepository.GetActiveChatbotConversationAsync(conversationId, cancellationToken), cancellationToken);
        return _mapper.Map<ChatbotConversationsResponseDTO>(conversation);
    }
    
    public async Task<PagedResponse<ChatbotConversationsResponseDTO>> GetAllChatbotConversationByUserIdAsync(ulong userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var (conversations, totalCount) = await _chatbotConversationRepository.GetAllChatbotConversationsByUserIdAsync(userId, page, pageSize, cancellationToken);
        var conversationDtos = _mapper.Map<List<ChatbotConversationsResponseDTO>>(conversations);
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        return new PagedResponse<ChatbotConversationsResponseDTO>
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
    
    public async Task<PagedResponse<ChatbotMessagesResponseDTO>> GetAllChatbotMessageByConversationIdAsync(ulong conversationId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var (messages, totalCount) = await _chatbotConversationRepository.GetAllChatbotMessagesByConversationIdAsync(conversationId, page, pageSize, cancellationToken);
        var messageDtos = _mapper.Map<List<ChatbotMessagesResponseDTO>>(messages);
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        return new PagedResponse<ChatbotMessagesResponseDTO>
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
}
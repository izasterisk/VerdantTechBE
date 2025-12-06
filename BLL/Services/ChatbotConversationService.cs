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

    public async Task<bool> SoftDeleteConversationAsync(ulong conversationId, ulong userId, CancellationToken ct = default)
    {
        var convo = await _chatbotConversationRepository.GetActiveChatbotConversationAsync(conversationId, ct);

        if (convo.CustomerId != userId)
            throw new UnauthorizedAccessException("Không có quyền xóa cuộc hội thoại này.");

        await _chatbotConversationRepository.SoftDeleteConversationAsync(conversationId, ct);
        return true;
    }

}
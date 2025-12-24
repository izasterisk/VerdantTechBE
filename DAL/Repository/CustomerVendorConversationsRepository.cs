using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository;

public class CustomerVendorConversationsRepository : ICustomerVendorConversationsRepository
{
    private readonly IRepository<CustomerVendorConversation> _customerVendorConversationsRepository;
    private readonly IRepository<CustomerVendorMessage> _customerVendorMessageRepository;
    private readonly IRepository<MediaLink> _mediaLinkRepository;
    private readonly VerdantTechDbContext _dbContext;
    private readonly IRepository<Product> _productRepository;
    
    public CustomerVendorConversationsRepository(
        IRepository<CustomerVendorConversation> customerVendorConversationsRepository,
        IRepository<CustomerVendorMessage> customerVendorMessageRepository,
        IRepository<MediaLink> mediaLinkRepository,
        VerdantTechDbContext dbContext,
        IRepository<Product> productRepository)
    {
        _customerVendorConversationsRepository = customerVendorConversationsRepository;
        _customerVendorMessageRepository = customerVendorMessageRepository;
        _mediaLinkRepository = mediaLinkRepository;
        _dbContext = dbContext;
        _productRepository = productRepository;
    }
    
    public async Task SendNewMessageAsync(CustomerVendorConversation conversation,
        CustomerVendorMessage message, List<MediaLink> images, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // conversation.LastMessageAt = DateTime.UtcNow;
            await _customerVendorConversationsRepository.UpdateAsync(conversation, cancellationToken);
            
            var createdMessage =  await _customerVendorMessageRepository.CreateAsync(message, cancellationToken);
            
            if(images.Count > 0)
            {
                foreach (var image in images)
                {
                    image.OwnerId = createdMessage.Id;
                    image.CreatedAt = DateTime.UtcNow;
                    image.UpdatedAt = DateTime.UtcNow;
                }
                await _mediaLinkRepository.CreateBulkAsync(images, cancellationToken);
            }
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
    
    public async Task<CustomerVendorConversation> GetOrCreateConversationByUserIdAsync(ulong customerId, ulong vendorId, UserRole senderRole, CancellationToken cancellationToken = default)
    {
        var con = await _customerVendorConversationsRepository.GetAsync
               (c => c.CustomerId == customerId && c.VendorId == vendorId, true, cancellationToken);
        if (con == null)
        {
            if(senderRole != UserRole.Customer)
                throw new InvalidOperationException("Chỉ khách hàng mới có thể bắt đầu cuộc trò chuyện với nhà cung cấp.");
            var create = new CustomerVendorConversation
            {
                CustomerId = customerId,
                VendorId = vendorId,
                StartedAt = DateTime.UtcNow
            };
            con = await _customerVendorConversationsRepository.CreateAsync(create, cancellationToken);
        }
        return con;
    }
    
    public async Task<List<MediaLink>> GetAllMessageImagesByIdAsync(ulong id, CancellationToken cancellationToken)
    {
        return await _dbContext.MediaLinks
            .AsNoTracking()
            .Where(ml => ml.OwnerType == MediaOwnerType.CustomerVendorMessages && ml.OwnerId == id)
            .OrderBy(ml => ml.SortOrder)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<CustomerVendorMessage> GetNewestMessageByConversationIdAsync(ulong conversationId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.CustomerVendorMessages
            .AsNoTracking()
            .Where(m => m.ConversationId == conversationId)
            .Include(m => m.Product)
            .OrderByDescending(m => m.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy tin nhắn nào trong cuộc trò chuyện này.");
    }
    
    public async Task<(List<CustomerVendorMessage>, int totalCount)> GetAllMessagesByConversationIdAsync
        (ulong conversationId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.CustomerVendorMessages.AsNoTracking()
            .Where(m => m.ConversationId == conversationId)
            .Include(m => m.Product);
        
        var totalCount = await query.CountAsync(cancellationToken);
        var messages = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return (messages, totalCount);
    }
    
    public async Task<List<MediaLink>> GetMediaLinksByOwnerIdsAsync(List<ulong> ownerIds, CancellationToken cancellationToken = default)
    {
        return await _dbContext.MediaLinks
            .AsNoTracking()
            .Where(ml => ml.OwnerType == MediaOwnerType.CustomerVendorMessages && ownerIds.Contains(ml.OwnerId))
            .OrderBy(ml => ml.OwnerId)
            .ThenBy(ml => ml.SortOrder)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<(List<CustomerVendorConversation>, int totalCount)> GetAllConversationsByUserIdAsync(ulong userId, int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.CustomerVendorConversations
            .AsNoTracking()
            .Where(c => c.CustomerId == userId || c.VendorId == userId)
            .Include(c => c.Vendor)
            .Include(c => c.Customer);
        
        var totalCount = await query.CountAsync(cancellationToken);
        
        var conversations = await query
            .OrderByDescending(c => c.LastMessageAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        
        return (conversations, totalCount);
    }
    
    public async Task ValidateProductBelongsToVendor(ulong productId, ulong vendorId, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetAsync(p => p.Id == productId, true, cancellationToken);
        if (product == null)
            throw new KeyNotFoundException("Sản phẩm không tồn tại.");
        if (product.VendorId != vendorId)
            throw new InvalidOperationException("Sản phẩm không thuộc về nhà cung cấp này.");
    }
}
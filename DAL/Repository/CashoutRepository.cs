using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository;

public class CashoutRepository : ICashoutRepository
{
    private readonly IRepository<Cashout> _cashoutRepository;
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<UserBankAccount> _userBankAccountRepository;
    private readonly IRepository<ProductSerial> _productSerialRepository; 
    private readonly IRepository<Request> _requestRepository;
    private readonly IRepository<ExportInventory> _exportInventoryRepository;
    private readonly IRepository<OrderDetail> _orderDetailRepository;
    private readonly VerdantTechDbContext _dbContext;
    private readonly IWalletRepository _walletRepository;
    
    public CashoutRepository(IRepository<Cashout> cashoutRepository, IRepository<Transaction> transactionRepository,
        IRepository<Order> orderRepository, IRepository<UserBankAccount> userBankAccountRepository,
        IRepository<ProductSerial> productSerialRepository, IRepository<Request> requestRepository,
        IRepository<ExportInventory> exportInventoryRepository, IRepository<OrderDetail> orderDetailRepository,
        VerdantTechDbContext dbContext, IWalletRepository walletRepository)
    {
        _cashoutRepository = cashoutRepository;
        _transactionRepository = transactionRepository;
        _orderRepository = orderRepository;
        _userBankAccountRepository = userBankAccountRepository;
        _productSerialRepository = productSerialRepository;
        _requestRepository = requestRepository;
        _exportInventoryRepository = exportInventoryRepository;
        _orderDetailRepository = orderDetailRepository;
        _dbContext = dbContext;
        _walletRepository = walletRepository;
    }

    public async Task<Cashout> CreateWalletCashoutAsync(Cashout cashout, Transaction tr, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var existing = await _walletRepository.GetWalletCashoutRequestByUserIdAsync(tr.UserId, cancellationToken);
            if (existing != null)
                throw new InvalidOperationException(
                    "Yêu cầu rút tiền đang chờ xử lý, vui lòng chờ đến khi yêu cầu trước được xử lý. " +
                    "Mỗi tài khoản chỉ được tồn tại 1 yêu cầu chưa được xử lý.");
            
            tr.CreatedAt = DateTime.UtcNow;
            tr.UpdatedAt = DateTime.UtcNow;
            var createdTransaction =  await _transactionRepository.CreateAsync(tr, cancellationToken);
            
            cashout.TransactionId = createdTransaction.Id;
            cashout.CreatedAt = DateTime.UtcNow;
            cashout.UpdatedAt = DateTime.UtcNow;
            var result = await _cashoutRepository.CreateAsync(cashout, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
    
    public async Task<Transaction> UpdateCashoutAsync(Cashout cashout, Transaction tr, CancellationToken cancellationToken = default)
    {
        cashout.UpdatedAt = DateTime.UtcNow;
        await _cashoutRepository.UpdateAsync(cashout, cancellationToken);
        
        tr.UpdatedAt = DateTime.UtcNow;
        return await _transactionRepository.UpdateAsync(tr, cancellationToken);
    }

    public async Task<Transaction> CreateRefundCashoutWithTransactionAsync(Transaction tr, Cashout cashout, 
        UserBankAccount? bankAccount, Order order, Request request, List<ProductSerial> serials, 
        List<ExportInventory> exports, List<OrderDetail> orderDetails, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            tr.CreatedAt = DateTime.UtcNow;
            tr.UpdatedAt = DateTime.UtcNow;
            var createdTransaction =  await _transactionRepository.CreateAsync(tr, cancellationToken);
            
            cashout.TransactionId = createdTransaction.Id;
            cashout.CreatedAt = DateTime.UtcNow;
            cashout.UpdatedAt = DateTime.UtcNow;
            await _cashoutRepository.CreateAsync(cashout, cancellationToken);

            if (bankAccount != null)
            {
                bankAccount.UpdatedAt = DateTime.UtcNow;
                await _userBankAccountRepository.UpdateAsync(bankAccount, cancellationToken);
            }
            
            order.UpdatedAt = DateTime.UtcNow;
            // order.Status = OrderStatus.Refunded;
            order.OrderDetails = null!;
            await _orderRepository.UpdateAsync(order, cancellationToken);
            
            request.UpdatedAt = DateTime.UtcNow;
            request.Status = RequestStatus.Completed;
            await _requestRepository.UpdateAsync(request, cancellationToken);
            
            foreach (var serial in serials)
            {
                serial.Status = ProductSerialStatus.Refund;
                serial.UpdatedAt = DateTime.UtcNow;
            }
            await _productSerialRepository.BulkUpdateAsync(serials, cancellationToken);
            
            await _exportInventoryRepository.BulkUpdateAsync(exports, cancellationToken);

            await _orderDetailRepository.BulkUpdateAsync(orderDetails, cancellationToken);
            
            await transaction.CommitAsync(cancellationToken);
            return createdTransaction;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
    
    public async Task<Transaction> GetCashoutRequestWithRelationsByTransactionIdAsync(ulong transactionId, CancellationToken cancellationToken = default) =>
        await _transactionRepository.GetWithRelationsAsync(c => c.Id == transactionId, true, 
            query => query.Include(u => u.BankAccount)
                .Include(u => u.CreatedByNavigation)
                .Include(u => u.ProcessedByNavigation)
                .Include(u => u.User)
                .Include(u => u.Cashout), cancellationToken) ?? 
        throw new KeyNotFoundException("Yêu cầu rút tiền không tồn tại.");
    
    public async Task<(Order, List<OrderDetail>, OrderStatus status)> GetOrderAndChosenOrderDetailsById(List<ulong> orderDetailIds, CancellationToken cancellationToken = default)
    {
        if(orderDetailIds.Count == 0)
            throw new ArgumentException("Danh sách OrderDetailIds không được để trống.");
        var orderDetails = await _dbContext.Set<OrderDetail>().Where(od => orderDetailIds.Contains(od.Id))
            .AsNoTracking().ToListAsync(cancellationToken);
        if(orderDetails.Count == 0)
            throw new KeyNotFoundException("Không tìm thấy OrderDetail nào khớp với danh sách ID cung cấp.");
        var count = 0;
        foreach (var orderDetail in orderDetails)
        {
            if(orderDetail.OrderId != orderDetails[0].OrderId)
                throw new InvalidOperationException("Tất cả OrderDetailIds phải thuộc về cùng một đơn hàng.");
            count++;
        }
        if(count != orderDetailIds.Count)
            throw new KeyNotFoundException("Một số OrderDetailIds không tồn tại.");
        var order = await _orderRepository.GetWithRelationsAsync(o => o.Id == orderDetails[0].OrderId, 
                        true, 
                        query => query.Include(o => o.OrderDetails), 
                        cancellationToken) 
            ?? throw new KeyNotFoundException("Đơn hàng không tồn tại.");
        var status = order.Status;
        order.Status = count == order.OrderDetails.Count ? OrderStatus.Refunded : OrderStatus.PartialRefund;
        return (order, orderDetails, status);
    }
    
    public async Task<List<ExportInventory>> GetAllExportInventoriesByOrderDetailIdsAsync(HashSet<ulong> orderDetailIds, CancellationToken cancellationToken = default)
    {
        if(orderDetailIds.Count == 0)
            throw new ArgumentException("Danh sách OrderDetailIds không được để trống.");
        var response = await _dbContext.Set<ExportInventory>()
            .Where(ei => ei.OrderDetailId.HasValue && orderDetailIds.Contains(ei.OrderDetailId.Value))
            .Include(ei => ei.ProductSerial)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        foreach(var export in response)
        {
            if(export.OrderDetailId != null && orderDetailIds.Contains(export.OrderDetailId.Value))
                orderDetailIds.Remove(export.OrderDetailId.Value);
            if (export.ProductSerial != null && export.ProductSerial.Status != ProductSerialStatus.Sold)
                throw new InvalidCastException($"Sản phẩm Serial '{export.ProductSerial.SerialNumber}' có trạng thái không hợp lệ: {export.ProductSerial.Status}. Yêu cầu trạng thái: 'Sold'.");
        }
        if(orderDetailIds.Count > 0)
            throw new KeyNotFoundException($"OrderDetail với ID này không có bản ghi xuất hàng: {string.Join(", ", orderDetailIds)}");
        return response;
    }
}
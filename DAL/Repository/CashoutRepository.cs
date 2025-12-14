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
    
    public async Task<List<ProductSerial>> GetSoldProductSerialsBySerialNumbersAsync(Dictionary<string, string> serials, CancellationToken cancellationToken = default)
    {
        if (serials.Count == 0)
        {
            return new List<ProductSerial>();
        }
        var serialKeys = serials.Keys.ToList();
        var foundSerials = await _dbContext.Set<ProductSerial>()
            .AsNoTracking()
            .Include(ps => ps.BatchInventory)
            .Where(ps => serialKeys.Contains(ps.SerialNumber))
            .ToListAsync(cancellationToken);
        
        var foundMap = foundSerials.ToDictionary(
            ps => ps.SerialNumber, 
            ps => ps, 
            StringComparer.OrdinalIgnoreCase
        );
        foreach (var req in serials)
        {
            var reqSerial = req.Key;
            var reqLotNumber = req.Value;

            if (!foundMap.TryGetValue(reqSerial, out var entity))
            {
                throw new KeyNotFoundException($"Serial number '{reqSerial}' không tồn tại trong hệ thống.");
            }
            if (entity.Status != ProductSerialStatus.Sold)
            {
                throw new InvalidOperationException($"Serial '{reqSerial}' có trạng thái không hợp lệ: {entity.Status}. Yêu cầu trạng thái: 'Sold'.");
            }
            var actualLotNumber = entity.BatchInventory.LotNumber;
            if (!string.Equals(actualLotNumber, reqLotNumber, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Serial '{reqSerial}' thuộc lô '{actualLotNumber}', không khớp với lô yêu cầu '{reqLotNumber}'.");
            }
        }
        return foundSerials;
    }
    
    public async Task<(Order, List<ExportInventory>)> ValidateExportedOrderByOrderDetailIdsAsync(
        Dictionary<(ulong OrderDetailId, string LotNumber), int> validateLotNumber, CancellationToken cancellationToken = default)
    {
        if (validateLotNumber == null || validateLotNumber.Count == 0)
        {
            throw new ArgumentException("Danh sách yêu cầu kiểm tra không được để trống.");
        }
        var orderDetailIds = validateLotNumber.Keys.Select(k => k.OrderDetailId).Distinct().ToList();
        var relevantLotNumbers = validateLotNumber.Keys.Select(k => k.LotNumber).Distinct().ToList();
    
        var orderDetailsWithOrder = await _dbContext.Set<OrderDetail>()
            .Include(od => od.Order) 
            .Where(od => orderDetailIds.Contains(od.Id))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        if (orderDetailsWithOrder.Count == 0)
            throw new KeyNotFoundException("Không tìm thấy OrderDetail nào khớp với danh sách ID cung cấp.");
        var distinctOrderIds = orderDetailsWithOrder.Select(od => od.OrderId).Distinct().ToList();
        if (distinctOrderIds.Count > 1)
        {
            throw new InvalidOperationException($"Dữ liệu không hợp lệ: Các chi tiết đơn hàng (OrderDetail) được cung cấp thuộc về {distinctOrderIds.Count} đơn hàng khác nhau.");
        }
        var order = orderDetailsWithOrder.First().Order;
    
        // Lưu ý: Query này có thể lấy dư một chút (nếu trùng LotNumber ở OrderDetail khác), nhưng ta sẽ lọc kỹ lại ở Memory.
        // Đây là cách nhanh nhất để tránh query trong vòng lặp foreach.
        var candidateExportInventories = await _dbContext.Set<ExportInventory>()
            .Where(ei => ei.OrderDetailId.HasValue && ei.MovementType == MovementType.Sale
                         && orderDetailIds.Contains(ei.OrderDetailId.Value) 
                         && relevantLotNumbers.Contains(ei.LotNumber))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    
        var validatedExportInventories = new List<ExportInventory>();
    
        // Duyệt qua Dictionary đầu vào để kiểm tra logic nghiệp vụ
        foreach (var req in validateLotNumber)
        {
            var (reqOrderDetailId, reqLotNumber) = req.Key;
            var reqQuantity = req.Value;
            
            var matchedExport = candidateExportInventories
                .FirstOrDefault(ei => ei.OrderDetailId == reqOrderDetailId && ei.LotNumber == reqLotNumber);
            if (matchedExport == null)
                throw new KeyNotFoundException($"Không tìm thấy dữ liệu xuất kho cho OrderDetail {reqOrderDetailId} với mã lô '{reqLotNumber}'.");
            // Tính số lượng khả dụng trên record duy nhất này
            var availableQty = matchedExport.Quantity - matchedExport.RefundQuantity;
            if (availableQty < reqQuantity)
            {
                throw new InvalidOperationException(
                    $"Số lượng không hợp lệ cho OrderDetail {reqOrderDetailId} (Lot: {reqLotNumber}). " +
                    $"Số lượng trong yêu cầu ({reqQuantity}) lớn hơn số lượng thực tế khả dụng ({availableQty}).");
            }
            matchedExport.RefundQuantity += reqQuantity;
            matchedExport.UpdatedAt = DateTime.UtcNow;
            validatedExportInventories.Add(matchedExport);
        }
        return (order, validatedExportInventories);
    }

    public async Task<decimal> GetTotalRefundedAmountByOrderDetailIdsAsync(Dictionary<(ulong OrderDetailId, string LotNumber), int> validateLotNumber, CancellationToken cancellationToken = default)
    {
        if (validateLotNumber.Count == 0)
        {
            return 0;
        }
        var orderDetailIds = validateLotNumber.Keys.Select(k => k.OrderDetailId).Distinct().ToList();
        var orderDetailMap = await _dbContext.Set<OrderDetail>()
            .AsNoTracking()
            .Where(od => orderDetailIds.Contains(od.Id))
            .Select(od => new 
            { 
                od.Id, 
                od.Subtotal, 
                od.Quantity 
            }) 
            .ToDictionaryAsync(x => x.Id, x => x, cancellationToken);
        decimal totalRefundAmount = 0;
        foreach (var req in validateLotNumber)
        {
            var orderDetailId = req.Key.OrderDetailId;
            var refundQuantity = req.Value;
            if (orderDetailMap.TryGetValue(orderDetailId, out var info))
            {
                decimal effectiveUnitPrice = info.Subtotal / info.Quantity;
                totalRefundAmount += effectiveUnitPrice * refundQuantity;
            }
        }
        return totalRefundAmount;
    }

    public async Task<(Order, List<OrderDetail>)> GetOrderAndChosenOrderDetailsById(List<ulong> orderDetailIds, CancellationToken cancellationToken = default)
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
        order.Status = count == order.OrderDetails.Count ? OrderStatus.Refunded : OrderStatus.PartialRefund;
        return (order, orderDetails);
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
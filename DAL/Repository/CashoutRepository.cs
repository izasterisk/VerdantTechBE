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
    private readonly IRepository<Payment> _paymentRepository;
    private readonly IRepository<ProductSerial> _productSerialRepository; 
    private readonly IRepository<Request> _requestRepository;
    private readonly VerdantTechDbContext _dbContext;
    private readonly IWalletRepository _walletRepository;
    
    public CashoutRepository(IRepository<Cashout> cashoutRepository, IRepository<Transaction> transactionRepository,
        IRepository<Order> orderRepository, IRepository<Payment> paymentRepository,
        IRepository<ProductSerial> productSerialRepository, IRepository<Request> requestRepository,
        VerdantTechDbContext dbContext, IWalletRepository walletRepository)
    {
        _cashoutRepository = cashoutRepository;
        _transactionRepository = transactionRepository;
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _productSerialRepository = productSerialRepository;
        _requestRepository = requestRepository;
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

    public async Task<Transaction> CreateRefundCashoutWithTransactionAsync(Cashout cashout, Transaction tr, 
        Order order, Request request, List<ProductSerial> serialIds, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            order.UpdatedAt = DateTime.UtcNow;
            order.Status = OrderStatus.Refunded;
            await _orderRepository.UpdateAsync(order, cancellationToken);
            
            request.UpdatedAt = DateTime.UtcNow;
            request.Status = RequestStatus.Completed;
            await _requestRepository.UpdateAsync(request, cancellationToken);
            
            var payment = await _transactionRepository.GetAsync(u => u.OrderId == order.Id, true, cancellationToken) ?? 
                throw new KeyNotFoundException("Không tìm thấy thanh toán liên quan đến đơn hàng.");
            if (payment.Status != TransactionStatus.Completed)
                throw new InvalidOperationException("Chỉ có thể hoàn tiền cho các thanh toán đã hoàn tất.");

            foreach (var serialId in serialIds)
            {
                serialId.Status = ProductSerialStatus.Refund;
                serialId.UpdatedAt = DateTime.UtcNow;
                await _productSerialRepository.UpdateAsync(serialId, cancellationToken);
            }
            
            tr.CreatedAt = DateTime.UtcNow;
            tr.UpdatedAt = DateTime.UtcNow;
            var createdTransaction =  await _transactionRepository.CreateAsync(tr, cancellationToken);
            
            cashout.TransactionId = createdTransaction.Id;
            cashout.CreatedAt = DateTime.UtcNow;
            cashout.UpdatedAt = DateTime.UtcNow;
            await _cashoutRepository.CreateAsync(cashout, cancellationToken);
            
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
                throw new InvalidOperationException(
                    $"Serial '{reqSerial}' thuộc lô '{actualLotNumber}', không khớp với lô yêu cầu '{reqLotNumber}'.");
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
            .Where(ei => ei.OrderDetailId.HasValue 
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
    
            // Tìm các bản ghi ExportInventory khớp chính xác trong list đã fetch từ DB
            var matchedExports = candidateExportInventories
                .Where(ei => ei.OrderDetailId == reqOrderDetailId && ei.LotNumber == reqLotNumber).ToList();
    
            // Tính tổng số lượng đã xuất kho (Exported Quantity) trừ số đã hoàn cho cặp ID/Lot này
            var totalExportedQty = matchedExports.Sum(x => x.Quantity - x.RefundQuantity);
            if (totalExportedQty < reqQuantity)
            {
                throw new InvalidOperationException(
                    $"Số lượng không hợp lệ cho OrderDetail {reqOrderDetailId} (Lot: {reqLotNumber}). " +
                    $"Số lượng trong yêu cầu ({reqQuantity}) lớn hơn số lượng thực tế đã xuất kho ({totalExportedQty}).");
            }
            validatedExportInventories.AddRange(matchedExports);
        }
        return (order, validatedExportInventories);
    }
    
    public async Task<List<ExportInventory>> GetSoldProductByLotNumbersAsync(Dictionary<string, int> validateLotNumber,
        ulong orderDetailId, CancellationToken cancellationToken = default)
    {
        var lotNumbersToCheck = validateLotNumber.Keys.ToList();
        var exportInventories = await _dbContext.Set<ExportInventory>()
            .Where(x => x.OrderDetailId == orderDetailId 
                        && lotNumbersToCheck.Contains(x.LotNumber))
            .ToListAsync(cancellationToken);

        if (exportInventories.Count != validateLotNumber.Count)
        {
            var foundLots = exportInventories
                .Select(x => x.LotNumber)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var missingLots = validateLotNumber.Keys
                .Where(k => !foundLots.Contains(k))
                .ToList();
            throw new KeyNotFoundException(
                $"Không tìm thấy thông tin xuất kho cho các mã lô (LotNumber) sau: {string.Join(", ", missingLots)}");
        }
        
        foreach (var item in exportInventories)
        {
            if (validateLotNumber.TryGetValue(item.LotNumber, out var requestedRefundQty))
            {
                var availableToRefund = item.Quantity - item.RefundQuantity;
                if (requestedRefundQty > availableToRefund)
                {
                    throw new InvalidOperationException(
                        $"Số lượng yêu cầu hoàn ({requestedRefundQty}) cho lô '{item.LotNumber}' " +
                        $"vượt quá số lượng khả dụng ({availableToRefund}). " +
                        $"(Đã mua: {item.Quantity}, Đã hoàn trước đó: {item.RefundQuantity})");
                }
            }
            else
            {
                // [Safety Net] Trường hợp này xảy ra khi SQL và C# có cơ chế so sánh chuỗi khác nhau:
                // 1. SQL Collation thường là CI/AI (bỏ qua dấu) hoặc tự động bỏ qua khoảng trắng cuối (Trailing Spaces).
                //    -> Ví dụ: DB có "LOT01 " (dư space) hoặc "café", SQL vẫn match với "LOT01" hoặc "cafe".
                // 2. C# Dictionary (OrdinalIgnoreCase) so sánh chặt chẽ hơn (khác dấu hoặc dư space được coi là khác nhau).
                // -> Throw lỗi để phát hiện dữ liệu rác trong DB thay vì bỏ qua âm thầm gây sai lệch tồn kho.
                throw new InvalidOperationException($"Lỗi dữ liệu: Tìm thấy lô '{item.LotNumber}' trong DB nhưng không khớp key trong danh sách yêu cầu.");
            }
        }
        return exportInventories;
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
}
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
    
    public async Task<List<ProductSerial>> GetSoldProductSerialsBySerialNumbersAsync(List<string> serialNumbers, CancellationToken cancellationToken = default)
    {
        if (serialNumbers.Count == 0)
        {
            return new List<ProductSerial>();
        }
        var foundSerials = await _dbContext.Set<ProductSerial>()
            .Where(ps => serialNumbers.Contains(ps.SerialNumber) && ps.Status == ProductSerialStatus.Sold)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (foundSerials.Count != serialNumbers.Count)
        {
            var foundSet = foundSerials.Select(x => x.SerialNumber).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var missingSerials = serialNumbers.Where(s => !foundSet.Contains(s));
            throw new KeyNotFoundException($"Các serial sau không hợp lệ (không tồn tại hoặc chưa bán): {string.Join(", ", missingSerials)}");
        }
        return foundSerials;
    }
    
    public async Task<Order> ValidateOrderByOrderDetailIdsAsync(List<ulong> orderDetailIds, List<ulong> checkSerialRequired, CancellationToken cancellationToken = default)
    {
        if (orderDetailIds == null || !orderDetailIds.Any())
        {
            throw new ArgumentException("Danh sách OrderDetailId không được để trống.");
        }
        var distinctOrderIds = await _dbContext.Set<OrderDetail>()
            .Where(od => orderDetailIds.Contains(od.Id))
            .Select(od => od.OrderId)
            .Distinct() // Đây là chìa khóa: SQL sẽ thực hiện SELECT DISTINCT
            .ToListAsync(cancellationToken);
        if (distinctOrderIds.Count > 1)
        {
            // Nếu List trả về > 1 phần tử => Có nhiều hơn 1 OrderId khác nhau
            throw new InvalidOperationException($"Các chi tiết đơn hàng (OrderDetail) được cung cấp thuộc về {distinctOrderIds.Count} đơn hàng khác nhau. Không hợp lệ.");
        }
        if (distinctOrderIds.Count == 0)
        {
            throw new KeyNotFoundException("Không tìm thấy OrderDetail nào khớp với danh sách ID cung cấp.");
        }
        if (checkSerialRequired.Count > 0)
        {
            var invalidProduct = await _dbContext.Set<OrderDetail>()
                .Where(od => checkSerialRequired.Contains(od.Id) && od.Product.Category.SerialRequired)
                .Select(od => od.Product.ProductName)
                .FirstOrDefaultAsync(cancellationToken);
            if (invalidProduct != null)
            {
                throw new InvalidOperationException(
                    $"Sản phẩm '{invalidProduct}' bắt buộc phải có số sê-ri.");
            }
        }
        return await _orderRepository.GetAsync(o => o.Id == distinctOrderIds.First(), true, cancellationToken)
            ?? throw new KeyNotFoundException($"Đơn hàng với ID {distinctOrderIds.First()} không tồn tại.");
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
}
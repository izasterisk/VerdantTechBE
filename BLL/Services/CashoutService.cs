using AutoMapper;
using BLL.DTO.Cashout;
using BLL.DTO.Order;
using BLL.DTO.Transaction;
using BLL.DTO.UserBankAccount;
using BLL.DTO.Wallet;
using BLL.Helpers.VendorBankAccounts;
using BLL.Interfaces;
using BLL.Interfaces.Infrastructure;
using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;

namespace BLL.Services;

public class CashoutService : ICashoutService
{
    private readonly IExportInventoryRepository _exportedInventoryRepository;
    private readonly IMapper _mapper;
    private readonly IPayOSApiClient _payOSApiClient;
    private readonly ICashoutRepository _cashoutRepository;
    private readonly IOrderDetailRepository _orderDetailRepository;
    private readonly IRequestRepository _requestRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUserBankAccountsRepository _userBankAccountRepository;
    private readonly INotificationService _notificationService;
    private readonly IExportInventoryService _exportInventoryService;
    
    public CashoutService(IExportInventoryRepository exportedInventoryRepository, IMapper mapper,
        IPayOSApiClient payOSApiClient, ICashoutRepository cashoutRepository,
        IOrderDetailRepository orderDetailRepository, IRequestRepository requestRepository,
        IOrderRepository orderRepository, IUserBankAccountsRepository userBankAccountRepository,
        INotificationService notificationService, IExportInventoryService exportInventoryService)
    {
        _exportedInventoryRepository = exportedInventoryRepository;
        _mapper = mapper;
        _payOSApiClient = payOSApiClient;
        _cashoutRepository = cashoutRepository;
        _orderDetailRepository = orderDetailRepository;
        _requestRepository = requestRepository;
        _orderRepository = orderRepository;
        _userBankAccountRepository = userBankAccountRepository;
        _notificationService = notificationService;
        _exportInventoryService = exportInventoryService;
    }
    
    public async Task<TransactionResponseDTO> CreateCashoutRefundAsync(ulong staffId, ulong requestId, RefundCreateDTO dtos, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dtos, $"{nameof(dtos)} rỗng.");
        var request = await _requestRepository.GetRequestByIdAsync(requestId, cancellationToken);
        if(request.Status != RequestStatus.Approved || request.RequestType != RequestType.RefundRequest)
            throw new InvalidDataException("Yêu cầu không đủ điều kiện để hoàn tiền.");
        if(dtos.OrderDetails.Count == 0)
            throw new InvalidDataException("Danh sách đơn hàng để hoàn tiền không được rỗng.");
        
        var orderDetailIdSeen = new HashSet<ulong>();
        var serialsCheck = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var serialsToUpdate = new List<ProductSerial>();
        var exportToUpdate = new List<ExportInventory>();
        var refundAmountEstimate = 0m;

        foreach (var orderDetail in dtos.OrderDetails)
        {
            if (!orderDetailIdSeen.Add(orderDetail.OrderDetailId))
                throw new InvalidOperationException($"OrderDetailId {orderDetail.OrderDetailId} bị lặp lại trong danh sách.");
        }
        var orderTuple = await _cashoutRepository.GetOrderAndChosenOrderDetailsById(orderDetailIdSeen.ToList(), cancellationToken);
        var exportsByOrderDetailId = await _cashoutRepository.GetAllExportInventoriesByOrderDetailIdsAsync(orderDetailIdSeen, cancellationToken);
        
        // Convert thành Dictionary search cho nhanh
        var orderDetailMap = orderTuple.Item2.ToDictionary(od => od.Id);
        var exports = exportsByOrderDetailId.GroupBy(e => e.OrderDetailId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());
        
        if(orderTuple.Item1.CustomerId != request.UserId)
            throw new UnauthorizedAccessException("Yêu cầu hoàn tiền không thuộc về người đặt hàng.");
        if(orderTuple.Item1.Status == OrderStatus.Refunded)
            throw new InvalidDataException("Đơn hàng đã được hoàn tiền trước đó.");
        if(orderTuple.Item1.Status != OrderStatus.Delivered || orderTuple.Item1.DeliveredAt == null)
            throw new InvalidDataException("Đơn hàng chưa được giao, không thể hoàn tiền.");
        if(orderTuple.Item1.DeliveredAt.Value.AddDays(7) < request.CreatedAt)
            throw new InvalidDataException("Không thể hoàn tiền cho đơn hàng đã quá 7 ngày kể từ khi giao hàng.");
        
        foreach (var orderDetail in dtos.OrderDetails)
        {
            if (!exports.TryGetValue(orderDetail.OrderDetailId, out var orderDetailExports) || orderDetailExports.Count == 0)
                throw new InvalidDataException($"OrderDetailId {orderDetail.OrderDetailId} không có bản ghi xuất kho nào.");
            var refundQuantityTotal = 0;
            var lotCheck = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var dto in orderDetail.IdentityNumbers)
            {
                if (orderDetailExports[0].ProductSerialId == null)
                {
                    if (dto.SerialNumber != null)
                        throw new InvalidDataException($"Sản phẩm trong đơn hàng ID {orderDetail.OrderDetailId} không có số sê-ri, không thể nhập số sê-ri.");
                    if (!lotCheck.Add(dto.LotNumber))
                        throw new InvalidOperationException($"Số Lô {dto.LotNumber} bị lặp lại trong danh sách.");
                    
                    var thisExport = orderDetailExports.FirstOrDefault(e => e.LotNumber.Trim().Equals(dto.LotNumber.Trim(), StringComparison.OrdinalIgnoreCase));
                    if (thisExport == null)
                        throw new InvalidDataException($"Không tìm thấy số lô '{dto.LotNumber}' cho OrderDetailId {orderDetail.OrderDetailId}.");
                    if (thisExport.Quantity - thisExport.RefundQuantity < dto.Quantity)
                        throw new InvalidDataException($"Số lượng xuất trong kho cho số lô '{dto.LotNumber}' không đủ để hoàn tiền cho OrderDetailId {orderDetail.OrderDetailId}.");
                    thisExport.RefundQuantity += dto.Quantity;
                    exportToUpdate.Add(thisExport);
                }
                else
                {
                    if (dto.SerialNumber == null)
                        throw new InvalidDataException($"Sản phẩm trong đơn hàng ID {orderDetail.OrderDetailId} có số sê-ri, phải nhập số sê-ri.");
                    if(dto.Quantity != 1)
                        throw new InvalidDataException("Với sản phẩm có số sê-ri, số lượng phải là 1.");
                    if (!serialsCheck.Add(dto.SerialNumber))
                        throw new InvalidOperationException($"Số sê-ri {dto.SerialNumber} bị lặp lại trong danh sách.");
                    
                    var thisSerial = orderDetailExports.FirstOrDefault
                        (e => e.LotNumber.Trim().Equals(dto.LotNumber.Trim(), StringComparison.OrdinalIgnoreCase)
                        && e.ProductSerial!.SerialNumber.Equals(dto.SerialNumber, StringComparison.OrdinalIgnoreCase));
                    if (thisSerial == null)
                        throw new InvalidDataException($"Không tìm thấy số sê-ri '{dto.SerialNumber}' với số lô '{dto.LotNumber}' cho OrderDetailId {orderDetail.OrderDetailId}.");
                    serialsToUpdate.Add(thisSerial.ProductSerial!);
                    thisSerial.RefundQuantity += dto.Quantity;
                    exportToUpdate.Add(thisSerial);
                }
                refundQuantityTotal += dto.Quantity;
            }
            var orderDetailDb = orderDetailMap[orderDetail.OrderDetailId];
            if(refundQuantityTotal > orderDetailDb.Quantity)
                throw new InvalidDataException($"Tổng số lượng hoàn tiền vượt quá số lượng đã mua cho OrderDetailId {orderDetail.OrderDetailId}.");
            refundAmountEstimate += (orderDetailDb.Quantity * orderDetailDb.UnitPrice - orderDetailDb.DiscountAmount) / orderDetailDb.Quantity * refundQuantityTotal;
        }
        
        if(dtos.RefundAmount > refundAmountEstimate)
            throw new InvalidDataException("Số tiền hoàn không được nhiều hơn số tiền của các đơn hàng.");
        
        var bankAccount = await _userBankAccountRepository.GetUserBankAccountByIdAsync(dtos.BankAccountId, cancellationToken);
        string cashoutResponseId;
        if (dtos.GatewayPaymentId == null)
        {
            var categories = new List<string> { "RefundCashout" };
            var cashoutResponse = await _payOSApiClient.CreateCashoutAsync(
                _mapper.Map<UserBankAccountResponseDTO>(bankAccount),
                dtos.RefundAmount,
                $"RefundCashout",
                categories, cancellationToken);
            cashoutResponseId = cashoutResponse.Id;
            bankAccount.OwnerName = cashoutResponse.ToAccountName;
        }
        else
            cashoutResponseId = dtos.GatewayPaymentId;
        
        var cashout = new Cashout
        {
            ReferenceType = CashoutReferenceType.Refund,
            ReferenceId = request.Id,
            Notes = $"Hoàn tiền với yêu cầu ID {request.Id}."
        };
        var transaction = new Transaction
        {
            TransactionType = TransactionType.Refund,
            Amount = dtos.RefundAmount,
            Currency = "VND",
            UserId = request.UserId,
            BankAccountId = bankAccount.Id,
            Status = TransactionStatus.Completed,
            Note = "Yêu cầu hoàn tiền",
            GatewayPaymentId = cashoutResponseId,
            CreatedBy = request.UserId,
            ProcessedBy = staffId,
            ProcessedAt = DateTime.UtcNow
        };
        var created = await _cashoutRepository.CreateRefundCashoutWithTransactionAsync(transaction, cashout, bankAccount, orderTuple.Item1, request, serialsToUpdate, exportToUpdate, cancellationToken);
        var cashoutRes = await _cashoutRepository.GetCashoutRequestWithRelationsByTransactionIdAsync(created.Id, cancellationToken);
        
        await _notificationService.CreateAndSendNotificationAsync(
            request.UserId,
            "Hoàn tiền thành công",
            $"Yêu cầu hoàn tiền của bạn đã được xử lý thành công. Số tiền {dtos.RefundAmount:N0} VNĐ đã được chuyển vào tài khoản ngân hàng của bạn.",
            NotificationReferenceType.Refund,
            created.Id,
            cancellationToken);
        return _mapper.Map<TransactionResponseDTO>(cashoutRes);
    }
    
    public async Task<(string IPv4, string IPv6)> GetIPAddressAsync(CancellationToken cancellationToken = default)
    {
        return await _payOSApiClient.GetIPAddressAsync(cancellationToken);
    }

    public async Task<decimal> GetBalanceAsync(CancellationToken cancellationToken = default)
    {
        return await _payOSApiClient.GetBalanceAsync(cancellationToken);
    }
}
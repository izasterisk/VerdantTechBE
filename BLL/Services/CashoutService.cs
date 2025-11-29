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
    
    public CashoutService(IExportInventoryRepository exportedInventoryRepository, IMapper mapper,
        IPayOSApiClient payOSApiClient, ICashoutRepository cashoutRepository,
        IOrderDetailRepository orderDetailRepository, IRequestRepository requestRepository,
        IOrderRepository orderRepository, IUserBankAccountsRepository userBankAccountRepository,
        INotificationService notificationService)
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
    }
    
    public async Task<TransactionResponseDTO> CreateCashoutRefundAsync(ulong staffId, ulong requestId, RefundCreateDTO dtos, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dtos, $"{nameof(dtos)} rỗng.");
        var request = await _requestRepository.GetRequestByIdAsync(requestId, cancellationToken);
        if(request.Status != RequestStatus.Approved || request.RequestType != RequestType.RefundRequest)
            throw new InvalidDataException("Yêu cầu không đủ điều kiện để hoàn tiền.");
        
        Dictionary<string, string> serials = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<(ulong OrderDetailId, string LotNumber), int> validateLotNumber = new();
        var orderDetailIdSeen = new HashSet<ulong>();
        foreach (var orderDetail in dtos.OrderDetails)
        {
            if(!orderDetailIdSeen.Add(orderDetail.OrderDetailId))
                throw new InvalidOperationException($"OrderDetailId {orderDetail.OrderDetailId} bị lặp lại trong danh sách.");
            var isThisMachine = false;
            var isThisNotMachine = false;
            foreach (var dto in orderDetail.IdentityNumbers)
            {
                if (dto.SerialNumber != null)
                {
                    isThisMachine = true;
                    if(isThisNotMachine)
                        throw new InvalidOperationException($"Đơn hàng ID {orderDetail.OrderDetailId} tất cả phải cùng có số sê-ri hoặc tất cả đều không có.");
                    if(dto.Quantity != 1)
                        throw new InvalidOperationException("Với sản phẩm có số sê-ri, số lượng phải là 1.");
                    if (serials.TryGetValue(dto.SerialNumber, out var serial))
                        throw new InvalidOperationException($"Số sê-ri {dto.SerialNumber} bị lặp lại trong danh sách.");
                    serials.Add(dto.SerialNumber, dto.LotNumber);
                }
                else
                {
                    isThisNotMachine = true;
                    if(isThisMachine)
                        throw new InvalidOperationException($"Đơn hàng ID {orderDetail.OrderDetailId} tất cả phải cùng có số sê-ri hoặc tất cả đều không có.");
                    var compositeKey = (orderDetail.OrderDetailId, dto.LotNumber.Trim().ToUpper());
                    if (validateLotNumber.TryGetValue(compositeKey, out var count))
                        throw new InvalidOperationException($"Số lô {dto.LotNumber} cho đơn hàng ID {orderDetail.OrderDetailId} bị lặp lại, vui lòng gộp số lượng.");
                    validateLotNumber[compositeKey] = dto.Quantity;
                }
            }
        }
        var orderRefund = await _cashoutRepository.ValidateExportedOrderByOrderDetailIdsAsync(validateLotNumber, cancellationToken);
        var serialProducts = await _cashoutRepository.GetSoldProductSerialsBySerialNumbersAsync(serials, cancellationToken);
        
        if(orderRefund.Item1.CustomerId != request.UserId)
            throw new UnauthorizedAccessException("Yêu cầu hoàn tiền không thuộc về người đặt hàng.");
        if(orderRefund.Item1.Status == OrderStatus.Refunded)
            throw new InvalidDataException("Đơn hàng đã được hoàn tiền trước đó.");
        if(orderRefund.Item1.Status != OrderStatus.Delivered || orderRefund.Item1.DeliveredAt == null)
            throw new InvalidDataException("Đơn hàng chưa được giao, không thể hoàn tiền.");
        if(orderRefund.Item1.DeliveredAt.Value.AddDays(7) < request.CreatedAt)
            throw new InvalidDataException("Không thể hoàn tiền cho đơn hàng đã quá 7 ngày kể từ khi giao hàng.");

        var refundAmountEstimate = await _cashoutRepository.GetTotalRefundedAmountByOrderDetailIdsAsync(validateLotNumber, cancellationToken);
        if(dtos.RefundAmount > refundAmountEstimate * 2)
            throw new InvalidDataException("Số tiền hoàn không được gấp đôi số tiền của các đơn hàng.");
        
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
            Note = "Yêu cầu rút tiền từ ví người bán",
            GatewayPaymentId = cashoutResponseId,
            CreatedBy = request.UserId,
            ProcessedBy = staffId,
            ProcessedAt = DateTime.UtcNow
        };
        var created = await _cashoutRepository.CreateRefundCashoutWithTransactionAsync(cashout, transaction, orderRefund, request, serials, cancellationToken);
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
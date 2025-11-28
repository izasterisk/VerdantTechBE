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
    
    public async Task<RefundReponseDTO> CreateCashoutRefundAsync(ulong staffId, ulong requestId, RefundCreateDTO dtos, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dtos, $"{nameof(dtos)} rỗng.");
        var request = await _requestRepository.GetRequestByIdAsync(requestId, cancellationToken);
        if(request.Status != RequestStatus.Approved || request.RequestType != RequestType.RefundRequest)
            throw new InvalidDataException("Yêu cầu không đủ điều kiện để hoàn tiền.");
        
        var serials = new HashSet<string>();
        var orderDetailIds = new HashSet<ulong>();
        var checkSerialRequired = new HashSet<ulong>();
        Dictionary<string, (ulong, int)> validateLotNumber = new(StringComparer.OrdinalIgnoreCase);
        foreach (var dto in dtos.OrderDetails)
        {
            orderDetailIds.Add(dto.OrderDetailId);
            if (dto.SerialNumber != null)
            {
                if(dto.Quantity != 1)
                    throw new InvalidOperationException("Với sản phẩm có số sê-ri, số lượng phải là 1.");
                if(!serials.Add(dto.SerialNumber.ToUpper()))
                    throw new InvalidOperationException($"Số sê-ri {dto.SerialNumber} bị lặp lại trong danh sách.");
            }
            else
            {
                checkSerialRequired.Add(dto.OrderDetailId);
                if (validateLotNumber.TryGetValue(dto.LotNumber, out var count))
                {
                    validateLotNumber[dto.LotNumber] = count + dto.Quantity;
                }
                else
                {
                    validateLotNumber[dto.LotNumber] = dto.Quantity;
                }
            }
        }
        var order = await _cashoutRepository.ValidateOrderByOrderDetailIdsAsync(orderDetailIds.ToList(), checkSerialRequired.ToList(), cancellationToken);
        var serialProducts = await _cashoutRepository.GetSoldProductSerialsBySerialNumbersAsync(serials.ToList(), cancellationToken);
        var exportInventories = await _cashoutRepository.GetSoldProductByLotNumbersAsynca
        
        if(order.CustomerId != request.UserId)
            throw new UnauthorizedAccessException("Yêu cầu hoàn tiền không thuộc về người đặt hàng.");
        if(order.Status == OrderStatus.Refunded)
            throw new InvalidDataException("Đơn hàng đã được hoàn tiền trước đó.");
        if(order.Status != OrderStatus.Delivered || order.DeliveredAt == null)
            throw new InvalidDataException("Đơn hàng chưa được giao, không thể hoàn tiền.");
        if(order.DeliveredAt.Value.AddDays(7) < request.CreatedAt)
            throw new InvalidDataException("Không thể hoàn tiền cho đơn hàng đã quá 7 ngày kể từ khi giao hàng.");
        
        var products = _mapper.Map<List<OrderDetailsResponseDTO>>(orderDetails);
        decimal totalAmount = 0.00m;
        foreach(var item in products)
        {
            totalAmount += item.Subtotal;
            item.Product.Images = _mapper.Map<List<ProductImageResponseDTO>>(
                await _orderRepository.GetProductImagesByProductIdAsync(item.Product.Id, cancellationToken));
        }
        if(dtos.RefundAmount > totalAmount * 2)
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
        var created = await _cashoutRepository.CreateRefundCashoutWithTransactionAsync(cashout, transaction, order, request, serials, cancellationToken);
        var cashoutRes = await _cashoutRepository.GetCashoutRequestWithRelationsByTransactionIdAsync(created.Id, cancellationToken);
        var reponseDto = new RefundReponseDTO();
        reponseDto.TransactionInfo = _mapper.Map<TransactionResponseDTO>(cashoutRes);
        reponseDto.OrderDetails = products;
        
        await _notificationService.CreateAndSendNotificationAsync(
            request.UserId,
            "Hoàn tiền thành công",
            $"Yêu cầu hoàn tiền của bạn đã được xử lý thành công. Số tiền {dtos.RefundAmount:N0} VNĐ đã được chuyển vào tài khoản ngân hàng của bạn.",
            NotificationReferenceType.Refund,
            created.Id,
            cancellationToken);
        return reponseDto;
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
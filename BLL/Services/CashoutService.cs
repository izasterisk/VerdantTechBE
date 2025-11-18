using AutoMapper;
using BLL.DTO.Cashout;
using BLL.DTO.Order;
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
    private readonly IWalletRepository _walletRepository;
    private readonly IMapper _mapper;
    private readonly IPayOSApiClient _payOSApiClient;
    private readonly ICashoutRepository _cashoutRepository;
    private readonly IOrderDetailRepository _orderDetailRepository;
    private readonly IRequestRepository _requestRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUserBankAccountsRepository _userBankAccountRepository;
    private readonly INotificationService _notificationService;
    
    public CashoutService(IWalletRepository walletRepository, IMapper mapper, IPayOSApiClient payOSApiClient,
        ICashoutRepository cashoutRepository, IOrderDetailRepository orderDetailRepository, IRequestRepository requestRepository,
        IOrderRepository orderRepository, IUserBankAccountsRepository userBankAccountsRepository, INotificationService notificationService)
    {
        _walletRepository = walletRepository;
        _mapper = mapper;
        _payOSApiClient = payOSApiClient;
        _cashoutRepository = cashoutRepository;
        _orderDetailRepository = orderDetailRepository;
        _requestRepository = requestRepository;
        _orderRepository = orderRepository;
        _userBankAccountRepository = userBankAccountsRepository;
        _notificationService = notificationService;
    }
    
    public async Task<RefundReponseDTO> CreateCashoutRefundByPayOSAsync(ulong staffId, ulong requestId, RefundCreateDTO dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} rỗng.");
        if(dto.GatewayPaymentId != null)
            throw new InvalidDataException("GatewayPaymentId không cần thiết trong chức năng này.");
        var request = await _requestRepository.GetRequestByIdAsync(requestId, cancellationToken);
        if(request.Status != RequestStatus.Approved || request.RequestType != RequestType.RefundRequest)
            throw new InvalidDataException("Yêu cầu không đủ điều kiện để hoàn tiền.");
    
        var orderDetails = await _orderDetailRepository.GetListedOrderDetailsByIdAsync(dto.OrderDetailId, cancellationToken);
        var order = await _orderRepository.GetOrderByIdAsync(orderDetails[0].OrderId, cancellationToken);
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
        if(dto.RefundAmount > totalAmount * 2)
            throw new InvalidDataException("Số tiền hoàn không được gấp đôi số tiền của các đơn hàng.");
        
        VendorBankAccountsHelper.ValidateBankCode(dto.UserBankAccount.BankCode);
        var bankAccount = await _userBankAccountRepository.GetExistedBankAccount(request.UserId, dto.UserBankAccount.AccountNumber, cancellationToken);
        if (bankAccount == null)
        {
            var userBankAccount = _mapper.Map<UserBankAccount>(dto.UserBankAccount);
            userBankAccount.UserId = request.UserId;
            bankAccount = await _userBankAccountRepository.CreateUserBankAccountWithTransactionAsync(userBankAccount, cancellationToken);
        }
        
        var categories = new List<string> { "RefundCashout" };
        var cashoutResponse = await _payOSApiClient.CreateCashoutAsync(
            _mapper.Map<UserBankAccountResponseDTO>(bankAccount), 
            dto.RefundAmount, 
            $"RefundCashout", 
            categories, cancellationToken);
        
        Transaction transaction = new Transaction
        {
            TransactionType = TransactionType.Refund,
            Amount = dto.RefundAmount,
            Currency = "VND",
            UserId = request.UserId,
            Status = TransactionStatus.Completed,
            Note = $"Hoàn tiền với yêu cầu ID {request.Id}",
            GatewayPaymentId = cashoutResponse.Id,
            CreatedBy = request.UserId,
            ProcessedBy = staffId,
            CompletedAt = DateTime.UtcNow
        };
        Cashout cashout = new Cashout
        {
            UserId = request.UserId,
            BankAccountId = bankAccount.Id,
            Amount = dto.RefundAmount,
            Status = CashoutStatus.Completed,
            ReferenceType = CashoutReferenceType.Refund,
            ReferenceId = request.Id,
            Notes = $"Hoàn tiền với yêu cầu ID {request.Id}",
            ProcessedBy = staffId,
            ProcessedAt = DateTime.UtcNow,
        };
        var created = await _cashoutRepository.CreateRefundCashoutWithTransactionAsync(cashout, transaction, order, cancellationToken);
        var cashoutRes = await _cashoutRepository.GetCashoutRequestWithRelationsByIdAsync(created.Id, cancellationToken);
        RefundReponseDTO reponseDto = new RefundReponseDTO();
        reponseDto.TransactionInfo = _mapper.Map<WalletCashoutResponseDTO>(cashoutRes);
        reponseDto.TransactionInfo.ToAccountName = cashoutResponse.ToAccountName;
        reponseDto.OrderDetails = products;
        
        await _notificationService.CreateAndSendNotificationAsync(
            request.UserId,
            "Rút tiền thành công",
            $"Yêu cầu rút tiền của bạn đã được xử lý thành công. Số tiền {dto.RefundAmount:N0} VNĐ đã được chuyển vào tài khoản ngân hàng của bạn.",
            NotificationReferenceType.Refund,
            created.Id,
            cancellationToken);
        return reponseDto;
    }

    public async Task<RefundReponseDTO> CreateCashoutRefundAsync(ulong staffId, ulong requestId, RefundCreateDTO dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} rỗng.");
        if(dto.GatewayPaymentId == null)
            throw new InvalidDataException("GatewayPaymentId là bắt buộc.");
        var request = await _requestRepository.GetRequestByIdAsync(requestId, cancellationToken);
        if(request.Status != RequestStatus.Approved || request.RequestType != RequestType.RefundRequest)
            throw new InvalidDataException("Yêu cầu không đủ điều kiện để hoàn tiền.");
    
        var orderDetails = await _orderDetailRepository.GetListedOrderDetailsByIdAsync(dto.OrderDetailId, cancellationToken);
        var order = await _orderRepository.GetOrderByIdAsync(orderDetails[0].OrderId, cancellationToken);
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
        if(dto.RefundAmount > totalAmount * 2)
            throw new InvalidDataException("Số tiền hoàn không được gấp đôi số tiền của các đơn hàng.");
        
        VendorBankAccountsHelper.ValidateBankCode(dto.UserBankAccount.BankCode);
        var bankAccount = await _userBankAccountRepository.GetExistedBankAccount(request.UserId, dto.UserBankAccount.AccountNumber, cancellationToken);
        if (bankAccount == null)
        {
            var userBankAccount = _mapper.Map<UserBankAccount>(dto.UserBankAccount);
            userBankAccount.UserId = request.UserId;
            bankAccount = await _userBankAccountRepository.CreateUserBankAccountWithTransactionAsync(userBankAccount, cancellationToken);
        }
    
        Transaction transaction = new Transaction
        {
            TransactionType = TransactionType.Refund,
            Amount = dto.RefundAmount,
            Currency = "VND",
            UserId = request.UserId,
            Status = TransactionStatus.Completed,
            Note = $"Hoàn tiền với yêu cầu ID {request.Id}",
            GatewayPaymentId = dto.GatewayPaymentId,
            CreatedBy = request.UserId,
            ProcessedBy = staffId,
            CompletedAt = DateTime.UtcNow
        };
        Cashout cashout = new Cashout
        {
            UserId = request.UserId,
            BankAccountId = bankAccount.Id,
            Amount = dto.RefundAmount,
            Status = CashoutStatus.Completed,
            ReferenceType = CashoutReferenceType.Refund,
            ReferenceId = request.Id,
            Notes = $"Hoàn tiền với yêu cầu ID {request.Id}",
            ProcessedBy = staffId,
            ProcessedAt = DateTime.UtcNow,
        };
        var created = await _cashoutRepository.CreateRefundCashoutWithTransactionAsync(cashout, transaction, order, cancellationToken);
        var cashoutRes = await _cashoutRepository.GetCashoutRequestWithRelationsByIdAsync(created.Id, cancellationToken);
        RefundReponseDTO reponseDto = new RefundReponseDTO();
        reponseDto.TransactionInfo = _mapper.Map<WalletCashoutResponseDTO>(cashoutRes);
        reponseDto.OrderDetails = products;
        
        await _notificationService.CreateAndSendNotificationAsync(
            request.UserId,
            "Rút tiền thành công",
            $"Yêu cầu rút tiền của bạn đã được xử lý thành công. Số tiền {dto.RefundAmount:N0} VNĐ đã được chuyển vào tài khoản ngân hàng của bạn.",
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
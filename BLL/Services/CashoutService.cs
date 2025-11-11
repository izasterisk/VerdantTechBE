using AutoMapper;
using BLL.DTO.Cashout;
using BLL.DTO.Order;
using BLL.Interfaces;
using BLL.Interfaces.Infrastructure;
using DAL.Data;
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
    public readonly IUserBankAccountsService _userBankAccountsService;
    
    public CashoutService(IWalletRepository walletRepository, IMapper mapper, IPayOSApiClient payOSApiClient,
        ICashoutRepository cashoutRepository, IOrderDetailRepository orderDetailRepository,
        IRequestRepository requestRepository, IOrderRepository orderRepository,
        IUserBankAccountsService userBankAccountsService)
    {
        _walletRepository = walletRepository;
        _mapper = mapper;
        _payOSApiClient = payOSApiClient;
        _cashoutRepository = cashoutRepository;
        _orderDetailRepository = orderDetailRepository;
        _requestRepository = requestRepository;
        _orderRepository = orderRepository;
        _userBankAccountsService = userBankAccountsService;
    }
    
    public async Task CreateCashoutRefundByPayOSAsync(ulong requestId, RefundCreateDTO dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} rỗng.");
        var request = await _requestRepository.GetRequestByIdAsync(requestId, cancellationToken);
        if(request.Status != RequestStatus.Approved || request.RequestType != RequestType.RefundRequest)
            throw new InvalidDataException("Yêu cầu chưa đủ điều kiện để hoàn tiền.");

        var orderDetails = await _orderDetailRepository.GetListedOrderDetailsByIdAsync(dto.OrderDetailId, cancellationToken);
        var order = await _orderRepository.GetOrderByIdAsync(orderDetails[0].OrderId, cancellationToken);
        var products = _mapper.Map<List<OrderDetailsResponseDTO>>(orderDetails);
        decimal totalAmount = 0.00m;
        foreach(var item in products)
        {
            totalAmount += item.Subtotal;
            item.Product.Images = _mapper.Map<List<ProductImageResponseDTO>>(
                await _orderRepository.GetProductImagesByProductIdAsync(item.Product.Id, cancellationToken));
        }
        if(dto.RefundAmount > totalAmount*2)
            throw new InvalidDataException("Số tiền hoàn không được gấp đôi số tiền của các đơn hàng.");
        
        var bankAccount = await _userBankAccountsService.CreateUserBankAccountAsync(request.UserId,
            dto.UserBankAccount, cancellationToken);
        var categories = new List<string> { "RefundCashout" };
        var cashoutResponse = await _payOSApiClient.CreateCashoutAsync(
            bankAccount, 
            dto.RefundAmount, 
            $"RefundCashout", 
            categories, cancellationToken);
        if(cashoutResponse.State != "SUCCEEDED")
            throw new InvalidOperationException($"Yêu cầu rút tiền qua PayOS không thành công. Mã lỗi: {cashoutResponse.ErrorCode}, Tin nhắn lỗi: {cashoutResponse.ErrorMessage}");

        // Lưu thông tin cashout vào DB
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
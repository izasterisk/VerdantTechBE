using AutoMapper;
using BLL.DTO;
using BLL.DTO.Transaction;
using BLL.DTO.UserBankAccount;
using BLL.DTO.Wallet;
using BLL.Interfaces;
using BLL.Interfaces.Infrastructure;
using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;

namespace BLL.Services;

public class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepository;
    private readonly IMapper _mapper;
    private readonly IOrderRepository _orderRepository;
    private readonly IUserBankAccountsRepository _userBankAccountsRepository;
    private readonly ICashoutRepository _cashoutRepository;
    private readonly IPayOSApiClient _payOSApiClient;
    private readonly INotificationService _notificationService;
    
    public WalletService(IWalletRepository walletRepository, IMapper mapper, IOrderRepository orderRepository,
        IUserBankAccountsRepository userBankAccountsRepository, ICashoutRepository cashoutRepository,
        IPayOSApiClient payOSService, INotificationService notificationService)
    {
        _walletRepository = walletRepository;
        _mapper = mapper;
        _orderRepository = orderRepository;
        _userBankAccountsRepository = userBankAccountsRepository;
        _cashoutRepository = cashoutRepository;
        _payOSApiClient = payOSService;
        _notificationService = notificationService;
    }

    public async Task<WalletResponseDTO> ProcessWalletCreditsAsync(ulong userId, CancellationToken cancellationToken = default)
    {
        if (!await _walletRepository.ValidateVendorQualified(userId, cancellationToken))
            throw new KeyNotFoundException("Người dùng không tồn tại hoặc không đủ điều kiện.");
        
        var orderDetails = await _walletRepository.GetAllOrderDetailsAvailableForCreditAsync(userId, cancellationToken);
        if(orderDetails.Count > 0)
        {
            var wallet = await _walletRepository.GetWalletByUserIdAsync(userId, cancellationToken);
            foreach (var orderDetail in orderDetails)
            {
                orderDetail.IsWalletCredited = true;
                wallet.Balance += orderDetail.Subtotal * ((100 - orderDetail.Product.CommissionRate) / 100);
            }
            await _walletRepository.UpdateWalletAndOrderDetailsWithTransactionAsync(orderDetails, wallet, cancellationToken);
        }
        return _mapper.Map<WalletResponseDTO>(await _walletRepository.GetWalletByUserIdWithRelationsAsync(userId, cancellationToken));
    }
    
    public async Task<TransactionResponseDTO> CreateWalletCashoutRequestAsync(ulong userId, WalletCashoutRequestCreateDTO dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} rỗng.");
        if (await _walletRepository.ValidateVendorQualified(userId, cancellationToken) == false)
            throw new KeyNotFoundException("Người dùng không tồn tại hoặc không đủ điều kiện.");
        var bankAccount = await _userBankAccountsRepository.GetUserBankAccountByIdAsync(dto.BankAccountId, cancellationToken);
        if (bankAccount.UserId != userId)
            throw new UnauthorizedAccessException("Tài khoản ngân hàng không thuộc về người dùng này.");
        var wallet = await _walletRepository.GetWalletByUserIdAsync(userId, cancellationToken);
        if (dto.Amount > wallet.Balance)
            throw new InvalidOperationException("Số dư trong ví không đủ để thực hiện yêu cầu rút tiền.");

        var cashout = new Cashout
        {
            ReferenceType = CashoutReferenceType.VendorWithdrawal,
            ReferenceId = wallet.Id,
            Notes = dto.Notes
        };
        var transaction = new Transaction
        {
            TransactionType = TransactionType.WalletCashout,
            Amount = dto.Amount,
            Currency = "VND",
            UserId = userId,
            BankAccountId = dto.BankAccountId,
            Status = TransactionStatus.Pending,
            Note = "Yêu cầu rút tiền từ ví người bán",
            CreatedBy = userId
        };
        await _cashoutRepository.CreateWalletCashoutAsync(cashout, transaction, cancellationToken);
        
        var createdCashout = await _walletRepository.GetWalletCashoutRequestWithRelationsByUserIdAsync(userId, cancellationToken);
        return _mapper.Map<TransactionResponseDTO>(createdCashout);
    }
    
    public async Task<TransactionResponseDTO> ProcessWalletCashoutRequestAsync(ulong staffId, ulong userId, WalletProcessCreateDTO dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} rỗng.");
        var walletCashout = await _walletRepository.GetTransactionWithWalletCashoutRequestByUserIdAsync(userId, cancellationToken);
        if (walletCashout == null || walletCashout.Cashout == null)
            throw new KeyNotFoundException("Tài khoản này chưa có bất kì yêu cầu rút tiền nào.");
        if(walletCashout.BankAccount == null)
            throw new InvalidOperationException("Tài khoản ngân hàng không tồn tại.");
        if(dto.Status == TransactionStatus.Pending)
            throw new InvalidOperationException("Trạng thái không thể là 'Pending' khi xử lý yêu cầu rút tiền.");
        
        walletCashout.Status = dto.Status;
        walletCashout.ProcessedBy = staffId; walletCashout.ProcessedAt = DateTime.UtcNow;

        string title; string message;
        
        if (dto.Status == TransactionStatus.Completed)
        {
            if(dto.GatewayPaymentId == null)
                throw new InvalidOperationException("Khi hoàn thành yêu cầu rút tiền, mã giao dịch từ cổng thanh toán phải tồn tại.");
            if(dto.CancelReason != null)
                throw new InvalidOperationException("Khi hoàn thành yêu cầu rút tiền, lý do hủy không được phép tồn tại.");
            
            var wallet = await _walletRepository.GetWalletByUserIdAsync(userId, cancellationToken);
            wallet.Balance -= walletCashout.Amount;
            wallet.LastUpdatedBy = staffId;
            await _walletRepository.ProcessWalletCashoutRequestWithTransactionAsync(walletCashout, walletCashout.Cashout, wallet, walletCashout.BankAccount, cancellationToken);

            title = "Yêu cầu rút tiền đã được xử lý thành công.";
            message = $"Yêu cầu rút tiền từ ví của bạn đã được xử lý thành công. Số tiền {walletCashout.Amount:N0} VNĐ đã được chuyển vào tài khoản ngân hàng của bạn.";
        }
        else
        {
            if(dto.GatewayPaymentId == null)
                throw new InvalidOperationException("Khi hủy yêu cầu rút tiền, mã giao dịch từ cổng thanh toán phải tồn tại.");
            walletCashout.Note = dto.CancelReason ?? 
                throw new InvalidOperationException("Khi hủy yêu cầu rút tiền, lý do hủy phải tồn tại.");
            await _cashoutRepository.UpdateCashoutAsync(walletCashout.Cashout, walletCashout, cancellationToken);
            
            title = "Yêu cầu rút tiền thất bại.";
            message = $"Yêu cầu rút tiền từ ví của bạn đã bị hủy. Lý do: {walletCashout.Note}.";
        }
        
        var finalResponse = await _cashoutRepository.GetCashoutRequestWithRelationsByTransactionIdAsync(walletCashout.Id, cancellationToken);
        
        await _notificationService.CreateAndSendNotificationAsync(
            userId,
            title,
            message,
            NotificationReferenceType.WalletCashout,
            finalResponse.Id,
            cancellationToken);
        return _mapper.Map<TransactionResponseDTO>(finalResponse);
    }
    
    public async Task<TransactionResponseDTO> ProcessWalletCashoutRequestByPayOSAsync(ulong staffId, ulong userId, CancellationToken cancellationToken = default)
    {
        var walletCashout = await _walletRepository.GetTransactionWithWalletCashoutRequestByUserIdAsync(userId, cancellationToken);
        if (walletCashout == null || walletCashout.Cashout == null)
            throw new KeyNotFoundException("Tài khoản này chưa có bất kì yêu cầu rút tiền nào.");
        if(walletCashout.BankAccount == null)
            throw new InvalidOperationException("Tài khoản ngân hàng không tồn tại.");
        if(await _payOSApiClient.GetBalanceAsync(cancellationToken) < (int)Math.Ceiling(walletCashout.Amount))
            throw new InvalidOperationException("Số dư trong tài khoản PayOS không đủ để thực hiện yêu cầu rút tiền. Vui lòng liên hệ bộ phận quản lý.");
        await Task.Delay(2000, cancellationToken);
        
        var categories = new List<string> { "WalletCashout" };
        var cashoutResponse = await _payOSApiClient.CreateCashoutAsync(
            _mapper.Map<UserBankAccountResponseDTO>(walletCashout.BankAccount), 
            (int)Math.Ceiling(walletCashout.Amount), 
            $"WalletCashout", 
            categories, cancellationToken);
        walletCashout.BankAccount.OwnerName = cashoutResponse.ToAccountName;

        walletCashout.GatewayPaymentId = cashoutResponse.Id;
        walletCashout.Status = TransactionStatus.Completed;
        walletCashout.ProcessedBy = staffId; walletCashout.ProcessedAt = DateTime.UtcNow;
        
        var wallet = await _walletRepository.GetWalletByUserIdAsync(userId, cancellationToken);
        wallet.Balance -= walletCashout.Amount;
        wallet.LastUpdatedBy = staffId;
        
        var c = await _walletRepository.ProcessWalletCashoutRequestWithTransactionAsync(walletCashout, walletCashout.Cashout, wallet, walletCashout.BankAccount, cancellationToken);
        
        var finalResponse = await _cashoutRepository.GetCashoutRequestWithRelationsByTransactionIdAsync(c.Id, cancellationToken);
        var mapped = _mapper.Map<TransactionResponseDTO>(finalResponse);
        
        await _notificationService.CreateAndSendNotificationAsync(
            userId,
            "Yêu cầu rút tiền đã được xử lý thành công",
            $"Yêu cầu rút tiền từ ví của bạn đã được xử lý thành công. Số tiền {walletCashout.Amount:N0} VNĐ đã được chuyển vào tài khoản ngân hàng của bạn.",
            NotificationReferenceType.WalletCashout,
            c.Id,
            cancellationToken);
        
        return mapped;
    }
    
    public async Task<TransactionResponseDTO> GetWalletCashoutRequestAsync(ulong userId, CancellationToken cancellationToken = default)
    {
        var createdCashout = await _walletRepository.GetWalletCashoutRequestWithRelationsByUserIdAsync(userId, cancellationToken);
        if (createdCashout == null)
            throw new KeyNotFoundException("Tài khoản này chưa có bất kì yêu cầu rút tiền nào.");
        return _mapper.Map<TransactionResponseDTO>(createdCashout);
    }
    
    public async Task<bool> DeleteWalletCashoutRequestAsync(ulong userId, CancellationToken cancellationToken = default)
    {
        var existingCashout = await _walletRepository.GetTransactionWithWalletCashoutRequestByUserIdAsync(userId, cancellationToken);
        if (existingCashout == null || existingCashout.Cashout == null)
            throw new KeyNotFoundException("Tài khoản này chưa có bất kì yêu cầu rút tiền nào.");
        return await _walletRepository.DeleteCashoutWithTransactionAsync(existingCashout, existingCashout.Cashout, cancellationToken);
    }
    
    public async Task<PagedResponse<TransactionResponseDTO>> GetAllWalletCashoutRequestAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var (cashouts, totalCount) = await _walletRepository.GetAllWalletCashoutRequestAsync(page, pageSize, cancellationToken);
        var cashoutDtos = _mapper.Map<List<TransactionResponseDTO>>(cashouts);
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        return new PagedResponse<TransactionResponseDTO>
        {
            Data = cashoutDtos,
            CurrentPage = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalRecords = totalCount,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }
    
    public async Task<PagedResponse<TransactionResponseDTO>> GetAllWalletCashoutRequestByUserIdAsync(ulong userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var (cashouts, totalCount) = await _walletRepository.GetAllWalletCashoutRequestByUserIdAsync(userId, page, pageSize, cancellationToken);
        var cashoutDtos = _mapper.Map<List<TransactionResponseDTO>>(cashouts);
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        return new PagedResponse<TransactionResponseDTO>
        {
            Data = cashoutDtos,
            CurrentPage = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalRecords = totalCount,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }
}
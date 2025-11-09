using AutoMapper;
using BLL.DTO;
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
    
    public WalletService(IWalletRepository walletRepository, IMapper mapper, IOrderRepository orderRepository,
        IUserBankAccountsRepository userBankAccountsRepository, ICashoutRepository cashoutRepository,
        IPayOSApiClient payOSService)
    {
        _walletRepository = walletRepository;
        _mapper = mapper;
        _orderRepository = orderRepository;
        _userBankAccountsRepository = userBankAccountsRepository;
        _cashoutRepository = cashoutRepository;
        _payOSApiClient = payOSService;
    }

    public async Task<WalletResponseDTO> ProcessWalletCreditsAsync(ulong userId, CancellationToken cancellationToken = default)
    {
        if (await _walletRepository.ValidateVendorQualified(userId, cancellationToken) == false)
            throw new KeyNotFoundException("Người dùng không tồn tại hoặc không đủ điều kiện.");
        
        var wallet = await _walletRepository.GetWalletByUserIdAsync(userId, cancellationToken);
        var orderDetails = await _walletRepository.GetAllOrderDetailsAvailableForCreditAsync(userId, cancellationToken);
        decimal balance = 0;
        List<OrderDetail> update = new List<OrderDetail>();
        foreach (var orderDetail in orderDetails)
        {
            orderDetail.IsWalletCredited = true;
            update.Add(orderDetail);
            balance += orderDetail.Subtotal * ((100 - orderDetail.Product.CommissionRate) / 100);
        }
        wallet.Balance += balance;
        await _walletRepository.UpdateWalletAndOrderDetailsWithTransactionAsync(update, wallet, cancellationToken);
        return _mapper.Map<WalletResponseDTO>(await _walletRepository.GetWalletByUserIdWithRelationsAsync(userId, cancellationToken));
    }
    
    public async Task<WalletCashoutRequestResponseDTO> CreateWalletCashoutRequestAsync(ulong userId,
        WalletCashoutRequestCreateDTO dto, CancellationToken cancellationToken = default)
    {
        if (await _walletRepository.ValidateVendorQualified(userId, cancellationToken) == false)
            throw new KeyNotFoundException("Người dùng không tồn tại hoặc không đủ điều kiện.");
        var bankAccount = await _userBankAccountsRepository.GetUserBankAccountByIdAsync(dto.BankAccountId, cancellationToken);
        if (bankAccount.UserId != userId)
            throw new UnauthorizedAccessException("Tài khoản ngân hàng không thuộc về người dùng này.");
        var wallet = await _walletRepository.GetWalletByUserIdAsync(userId, cancellationToken);
        if (dto.Amount > wallet.Balance)
            throw new InvalidOperationException("Số dư trong ví không đủ để thực hiện yêu cầu rút tiền.");
        
        if (dto.Amount != 2000)
            throw new InvalidOperationException("Vì lý do kinh tế làm ơn chỉ rút 2000.");
        
        var cashout = _mapper.Map<Cashout>(dto);
        cashout.VendorId = userId; cashout.Status = CashoutStatus.Processing;
        cashout.ReferenceType = CashoutReferenceType.VendorWithdrawal; cashout.ReferenceId = wallet.Id;
        await _cashoutRepository.CreateCashoutForWalletCashoutAsync(cashout, cancellationToken);
        
        var createdCashout = await _walletRepository.GetWalletCashoutRequestWithRelationsByUserIdAsync(userId, cancellationToken);
        if (createdCashout == null)
            throw new KeyNotFoundException("Lỗi hệ thống khi tạo bản ghi, vui lòng liên hệ staff.");
        return _mapper.Map<WalletCashoutRequestResponseDTO>(createdCashout);
    }
    
    // public async Task<WalletCashoutRequestResponseDTO> ProcessWalletCashoutRequestAsync(ulong userId, WalletProcessCreateDTO dto, CancellationToken cancellationToken = default)
    // {
    //     var request = await _walletRepository.GetWalletCashoutRequestWithRelationsByUserIdAsync(userId, cancellationToken);
    //     if (request == null)
    //         throw new KeyNotFoundException("Tài khoản này chưa có bất kì yêu cầu rút tiền nào.");
    //     if (request.Status != CashoutStatus.Processing)
    //         throw new InvalidOperationException("Yêu cầu rút tiền này đã được xử lý.");
    //     
    //     
    //     var response = await _walletRepository.GetWalletCashoutRequestWithRelationsByUserIdAsync(userId, cancellationToken);
    //     return _mapper.Map<WalletCashoutRequestResponseDTO>(response);
    // }
    
    public async Task<WalletCashoutResponseDTO> ProcessWalletCashoutRequestByPayOSAsync(ulong staffId, ulong userId, CancellationToken cancellationToken = default)
    {
        var walletCashout = await _walletRepository.GetWalletCashoutRequestWithRelationsByUserIdAsync(userId, cancellationToken);
        if (walletCashout == null)
            throw new KeyNotFoundException("Tài khoản này chưa có bất kì yêu cầu rút tiền nào.");
        if (walletCashout.Status != CashoutStatus.Processing)
            throw new InvalidOperationException("Yêu cầu rút tiền này đã được xử lý.");
        
        // var categories = new List<string> { "WalletCashout" };
        // var cashoutResponse = await _payOSApiClient.CreateCashoutAsync(
        //     _mapper.Map<UserBankAccountResponseDTO>(walletCashout.BankAccount), 
        //     (int)Math.Ceiling(walletCashout.Amount), 
        //     $"WalletCashout", 
        //     categories, cancellationToken);
        Transaction transaction = new Transaction
        {
            TransactionType = TransactionType.WalletCashout,
            Currency = "VND",
            Amount = walletCashout.Amount,
            UserId = userId,
            Status = TransactionStatus.Completed,
            Note = $"Rút tiền từ ví người bán với yêu cầu ID {walletCashout.Id}",
            GatewayPaymentId = "cashoutResponse.Id",
            CreatedBy = userId,
            ProcessedBy = staffId,
            CompletedAt = DateTime.UtcNow
        };
        walletCashout.Status = CashoutStatus.Completed;
        walletCashout.ProcessedBy = staffId; walletCashout.ProcessedAt = DateTime.UtcNow;
        var c = await _walletRepository.ProcessWalletCashoutRequestAsync(transaction, walletCashout, cancellationToken);
        
        var finalResponse = await _walletRepository.GetWalletCashoutRequestWithRelationsByIdAsync(c.Id, cancellationToken);
        return _mapper.Map<WalletCashoutResponseDTO>(finalResponse);
    }
    
    public async Task<WalletCashoutRequestResponseDTO> GetWalletCashoutRequestAsync(ulong userId, CancellationToken cancellationToken = default)
    {
        var createdCashout = await _walletRepository.GetWalletCashoutRequestWithRelationsByUserIdAsync(userId, cancellationToken);
        if (createdCashout == null)
            throw new KeyNotFoundException("Tài khoản này chưa có bất kì yêu cầu rút tiền nào.");
        return _mapper.Map<WalletCashoutRequestResponseDTO>(createdCashout);
    }
    
    public async Task<bool> DeleteWalletCashoutRequestAsync(ulong userId, CancellationToken cancellationToken = default)
    {
        var existingCashout = await _walletRepository.GetWalletCashoutRequestByUserIdAsync(userId, cancellationToken);
        if (existingCashout == null)
            throw new KeyNotFoundException("Tài khoản này chưa có bất kì yêu cầu rút tiền nào.");
        return await _cashoutRepository.DeleteCashoutAsync(existingCashout, cancellationToken);
    }
    
    public async Task<PagedResponse<WalletCashoutRequestResponseDTO>> GetAllWalletCashoutRequestAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var (cashouts, totalCount) = await _walletRepository.GetAllWalletCashoutRequestAsync(page, pageSize, cancellationToken);
        var cashoutDtos = _mapper.Map<List<WalletCashoutRequestResponseDTO>>(cashouts);
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        return new PagedResponse<WalletCashoutRequestResponseDTO>
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
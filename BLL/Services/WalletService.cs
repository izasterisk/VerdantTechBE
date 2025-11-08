using AutoMapper;
using BLL.DTO.Wallet;
using BLL.Interfaces;
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
    
    public WalletService(IWalletRepository walletRepository, IMapper mapper,
        IOrderRepository orderRepository, IUserBankAccountsRepository userBankAccountsRepository,
        ICashoutRepository cashoutRepository)
    {
        _walletRepository = walletRepository;
        _mapper = mapper;
        _orderRepository = orderRepository;
        _userBankAccountsRepository = userBankAccountsRepository;
        _cashoutRepository = cashoutRepository;
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
        
        var cashout = _mapper.Map<Cashout>(dto);
        cashout.VendorId = userId; cashout.Status = CashoutStatus.Pending;
        cashout.ReferenceType = CashoutReferenceType.VendorWithdrawal; cashout.ReferenceId = wallet.Id;
        await _cashoutRepository.CreateCashoutForWalletCashoutAsync(cashout, cancellationToken);
        
        var createdCashout = await _walletRepository.GetWalletCashoutRequestWithRelationsByUserIdAsync(userId, cancellationToken);
        if (createdCashout == null)
            throw new KeyNotFoundException("Lỗi hệ thống khi tạo bản ghi, vui lòng liên hệ staff.");
        return _mapper.Map<WalletCashoutRequestResponseDTO>(createdCashout);
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
}
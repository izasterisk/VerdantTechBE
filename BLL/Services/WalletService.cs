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
    
    public WalletService(IMapper mapper, IWalletRepository walletRepository, IOrderRepository  orderRepository)
    {
        _mapper = mapper;
        _walletRepository = walletRepository;
        _orderRepository = orderRepository;
    }

    public async Task<WalletResponseDTO> ProcessWalletCreditsAsync(ulong userId, CancellationToken cancellationToken = default)
    {
        if (await _walletRepository.ValidateVendorQualified(userId, cancellationToken) == false)
            throw new KeyNotFoundException("Người dùng không tồn tại hoặc không phải vendor.");
        
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
}
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

    public async Task<WalletResponseDTO> GetWalletBalanceAsync(ulong vendorId, CancellationToken cancellationToken = default)
    {
        var wallet = await _walletRepository.GetWalletByVendorIdWithRelationsAsync(vendorId, cancellationToken);
        var orders = await _walletRepository.GetAllDeliveredOrdersAsync(cancellationToken);
        decimal balance = 0;
        List<Order> update = new List<Order>();
        foreach (var order in orders)
        {
            foreach (var orderDetail in order.OrderDetails)
            {
                if (orderDetail.Product.VendorId == vendorId)
                {
                    order.Status = OrderStatus.Finished;
                    update.Add(order);
                    if(orderDetail.Product.CommissionRate != 0)
                        balance += orderDetail.Subtotal * ((100 - orderDetail.Product.CommissionRate) / 100);
                }
            }
        }
        wallet.Balance += balance;
        await _walletRepository.UpdateOrdersWithTransactionAsync(update, cancellationToken);
        await _walletRepository.UpdateWalletWithTransactionAsync(wallet, cancellationToken);
        return _mapper.Map<WalletResponseDTO>(wallet);
    }
}
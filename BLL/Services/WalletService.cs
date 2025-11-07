using AutoMapper;
using BLL.DTO.Wallet;
using BLL.Interfaces;
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

    // public async Task<WalletResponseDTO> GetWalletBalanceAsync(ulong vendorId, CancellationToken cancellationToken = default)
    // {
    //     var wallet = await _walletRepository.GetWalletByVendorIdWithRelationsAsync(vendorId, cancellationToken);
    //     var orders = await _walletRepository.GetAllDeliveredOrdersAsync(cancellationToken);
    // }
}